// Program.fs
module JenkinsConfigurator

open System
open System.IO

// CT Insight 1: Algebraic Data Types for precise domain modeling
type Trigger =
    | Cron of schedule:string
    | PollSCM of schedule:string
    | Upstream of projects:string list * threshold:string

type Step =
    | Echo of message:string
    | Sh of script:string * options:ShOptions
    | Pwsh of script:string * options:PwshOptions
    | ArchiveArtifacts of pattern:string
    | JUnit of pattern:string
    | XUnit of toolType:string * pattern:string
    | WithCredentials of CredentialBinding list
    | TryFinally of mainSteps:Step list * finallySteps:Step list

and ShOptions = { ReturnStdout: bool; Label: string option }
and PwshOptions = { ReturnStdout: bool; Label: string option }
and CredentialBinding =
    | UsernamePassword of id:string * userVar:string * passVar:string
    | SecretText of id:string * variable:string
    | SshKey of id:string * keyFileVar:string

type Stage = { Name: string; Steps: Step list }

// FP Insight 1: Monadic builder for compositional configuration
type JenkinsPipeline = {
    Node: string option
    Triggers: Trigger list
    Stages: Stage list
}

// CT Insight 2: Monoidal empty element
let emptyPipeline = { Node = None; Triggers = []; Stages = [] }

// Computation Expression Builders
type PipelineBuilder() =
    member _.Yield(_) = emptyPipeline
    
    [<CustomOperation("node")>]
    member _.Node(pipeline, label) = { pipeline with Node = Some label }
    
    [<CustomOperation("trigger")>]
    member _.Trigger(pipeline, trigger) = { pipeline with Triggers = trigger :: pipeline.Triggers }
    
    [<CustomOperation("stage")>]
    member _.Stage(pipeline, name, steps) =
        { pipeline with Stages = { Name = name; Steps = steps } :: pipeline.Stages }
    
    member _.Run(pipeline) = { pipeline with Stages = List.rev pipeline.Stages }

type StepBuilder() =
    member _.Yield(_) = []
    
    // CT Insight 3: Preserving compositionality through concatenation
    member _.Combine(a, b) = a @ b
    
    member _.Delay(f) = f()
    
    [<CustomOperation("echo")>]
    member _.Echo(steps, msg) = Echo msg :: steps
    
    [<CustomOperation("sh")>]
    member _.Sh(steps, script, ?returnStdout, ?label) =
        Sh(script, { ReturnStdout = defaultArg returnStdout false; Label = label }) :: steps
    
    [<CustomOperation("pwsh")>]
    member _.Pwsh(steps, script, ?returnStdout, ?label) =
        Pwsh(script, { ReturnStdout = defaultArg returnStdout false; Label = label }) :: steps
    
    [<CustomOperation("archive")>]
    member _.Archive(steps, pattern) = ArchiveArtifacts pattern :: steps
    
    [<CustomOperation("junit")>]
    member _.JUnit(steps, pattern) = JUnit pattern :: steps
    
    [<CustomOperation("xunit")>]
    member _.XUnit(steps, tool, pattern) = XUnit(tool, pattern) :: steps
    
    [<CustomOperation("tryFinally")>]
    member _.TryFinally(steps, mainSteps, finallySteps) =
        TryFinally(mainSteps, finallySteps) :: steps
    
    [<CustomOperation("withCredentials")>]
    member _.WithCredentials(steps, bindings) = WithCredentials bindings :: steps

type CredentialBuilder() =
    member _.Yield(_) = []
    
    [<CustomOperation("usernamePassword")>]
    member _.AddUsernamePassword(bindings, id, userVar, passVar) =
        UsernamePassword(id, userVar, passVar) :: bindings
    
    [<CustomOperation("secretText")>]
    member _.AddSecretText(bindings, id, var) = SecretText(id, var) :: bindings
    
    [<CustomOperation("sshKey")>]
    member _.AddSshKey(bindings, id, keyVar) = SshKey(id, keyVar) :: bindings

// FP Insight 2: Recursive structural transformation
module GroovyGenerator =
    let rec private formatStep indentLevel step =
        let indent n = String.replicate (n * 4) " "
        match step with
        | Echo msg -> $"{indent indentLevel}echo '{msg}'"
        | Sh(script, opts) ->
            let options = [
                $"script: '{script}'"
                if opts.ReturnStdout then "returnStdout: true"
                match opts.Label with Some l -> $"label: '{l}'" | _ -> ()
            ]
            $"{indent indentLevel}sh {String.Join(", ", options)}"
        | Pwsh(script, opts) ->
            let options = [
                $"script: '{script}'"
                if opts.ReturnStdout then "returnStdout: true"
                match opts.Label with Some l -> $"label: '{l}'" | _ -> ()
            ]
            $"{indent indentLevel}pwsh {String.Join(", ", options)}"
        | ArchiveArtifacts p -> $"{indent indentLevel}archiveArtifacts artifacts: '{p}'"
        | JUnit p -> $"{indent indentLevel}junit '{p}'"
        | XUnit(t, p) -> $"{indent indentLevel}xUnit([$class: '{t}Parser', pattern: '{p}'])"
        | WithCredentials bindings ->
            let formattedBindings = bindings |> List.map (function
                | UsernamePassword(id, u, p) -> 
                    $"usernamePassword(credentialsId: '{id}', usernameVariable: '{u}', passwordVariable: '{p}')"
                | SecretText(id, v) -> $"string(credentialsId: '{id}', variable: '{v}')"
                | SshKey(id, k) -> $"sshUserPrivateKey(credentialsId: '{id}', keyFileVariable: '{k}')")
            $"""{indent indentLevel}withCredentials([{String.Join(", ", formattedBindings)}]) {{
{indent (indentLevel + 1)}// Credential usage here
{indent indentLevel}}}"""
        | TryFinally(main, finallySteps) ->
            let mainBlock = main |> List.collect (formatStep (indentLevel + 1)) |> String.concat "\n"
            let finallyBlock = finallySteps |> List.collect (formatStep (indentLevel + 1)) |> String.concat "\n"
            $"{indent indentLevel}try {{\n{mainBlock}\n{indent indentLevel}}} finally {{\n{finallyBlock}\n{indent indentLevel}}}"

    let generate pipeline =
        let triggers = pipeline.Triggers |> List.map (function
            | Cron s -> $"cron('{s}')"
            | PollSCM s -> $"pollSCM('{s}')"
            | Upstream(p, t) -> 
                let projects = p |> List.map (sprintf "'%s'") |> String.concat ", "
                $"upstream(upstreamProjects: [{projects}], threshold: hudson.model.Result.{t})")
        
        let stages = pipeline.Stages |> List.map (fun s ->
            let steps = s.Steps |> List.collect (formatStep 2)
            $"{String.replicate 4 " "}stage('{s.Name}') {{\n{String.Join("\n", steps)}\n{String.replicate 4 " "}}}")
        
        let nodeBlock = 
            match pipeline.Node with
            | Some label -> $"node('{label}') {{\n{String.Join("\n", stages)}\n}}"
            | None -> String.Join("\n", stages)
        
        if not (List.isEmpty triggers) then
            $"properties([\n    pipelineTriggers([\n{String.Join("\n", triggers |> List.map (sprintf "        %s"))}\n    ])\n])\n\n{nodeBlock}"
        else nodeBlock

// JCasC Configuration with Computation Expression
type JcascBuilder() =
    member _.Yield(_) = []
    
    [<CustomOperation("credential")>]
    member _.AddCredential(creds, id, config) = config id :: creds
    
    member _.Run(creds) = List.rev creds

type CredentialConfig =
    | UsernamePasswordCred of id:string * user:string * pass:string * desc:string
    | SecretTextCred of id:string * secret:string * desc:string

let jcasc = JcascBuilder()
let credential = CredentialBuilder()

let generateJcascYaml creds =
    let formatCred = function
        | UsernamePasswordCred(id, u, p, d) ->
            $"""- usernamePassword:
      scope: GLOBAL
      id: {id}
      username: {u}
      password: {p}
      description: "{d}" """
        | SecretTextCred(id, s, d) ->
            $"""- string:
      scope: GLOBAL
      id: {id}
      secret: {s}
      description: "{d}" """
    
    $"""jenkins:
  securityRealm: [...]
credentials:
  system:
    domainCredentials:
      - credentials:
{String.Join("\n", creds |> List.map (formatCred >> (sprintf "        %s"))}"""

// FP Insight 3: Program as composition of pure transformations
[<EntryPoint>]
let main _ =
    let pipeline = PipelineBuilder() {
        node "linux"
        trigger (Upstream(["base-pipeline"], "SUCCESS"))
        
        stage "Build" (StepBuilder() {
            echo "Starting build"
            sh "dotnet build"
            archive "**/bin/**/*.dll"
            tryFinally 
                (StepBuilder() { echo "In try block" })
                (StepBuilder() { echo "In finally block" })
        })
        
        stage "Deploy" (StepBuilder() {
            withCredentials (CredentialBuilder() {
                usernamePassword "docker-creds" "DOCKER_USER" "DOCKER_PASS"
            })
            pwsh "docker login -u $env:DOCKER_USER -p $env:DOCKER_PASS"
        })
    }

    let jcascConfig = jcasc {
        credential "docker-creds" (fun id -> 
            UsernamePasswordCred(id, "admin", "s3cr3t", "Docker Hub credentials"))
        credential "api-token" (fun id -> 
            SecretTextCred(id, "ABCD1234", "API Token"))
    }

    File.WriteAllText("Jenkinsfile", GroovyGenerator.generate pipeline)
    File.WriteAllText("jenkins.yaml", generateJcascYaml jcascConfig)
    0


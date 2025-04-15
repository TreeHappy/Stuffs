using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

public class Program
{
    public static void Main(string[] args)
    {
        // Generate JCasC configuration
        var jcascConfig = new JenkinsConfigAsCodeBuilder()
            .AddCredential(c => c.UsernamePassword("docker-creds", cred =>
            {
                cred.Username = "admin";
                cred.Password = "s3cr3t";
                cred.Description = "Docker Hub credentials";
            }))
            .AddCredential(c => c.SecretText("api-token", cred =>
            {
                cred.Secret = "ABCD1234";
                cred.Description = "API access token";
            }))
            .GenerateYaml();

        File.WriteAllText("jenkins.yaml", jcascConfig);
        Console.WriteLine("Generated jenkins.yaml file");

        // Generate Jenkins pipeline
        var pipeline = new JenkinsPipelineBuilder()
            .WithTriggers(t => t
                .WithCron("H */4 * * 1-5")
                .WithUpstream(u => u
                    .WithProjects("base-pipeline")
                    .WithThreshold("SUCCESS")))
            .WithNode("windows")
            .WithStage("Build", stage => stage
                .WithStep(step => step
                    .Sh("dotnet build")
                    .ArchiveArtifacts("**/bin/**/*.dll"))
                .WithFinally(cleanup => cleanup
                    .Echo("Cleaning build artifacts")
                    .Sh("dotnet clean")))
            .WithStage("Deploy", stage => stage
                .WithStep(step => step
                    .WithCredentials(creds => creds
                        .AddUsernamePassword("docker-creds", "DOCKER_USER", "DOCKER_PASS"))
                    .Pwsh("docker login -u $env:DOCKER_USER -p $env:DOCKER_PASS")
                    .ArchiveArtifacts("**/deploy.log")))
            .Build();

        File.WriteAllText("Jenkinsfile", pipeline);
        Console.WriteLine("Generated Jenkinsfile");
    }
}

public class JenkinsPipelineBuilder
{
    private readonly Pipeline _pipeline = new Pipeline();

    public JenkinsPipelineBuilder WithNode(string label)
    {
        _pipeline.Node = new Node(label);
        return this;
    }

    public JenkinsPipelineBuilder WithTriggers(Action<TriggersBuilder> configure)
    {
        var builder = new TriggersBuilder();
        configure(builder);
        _pipeline.Triggers = builder.Build();
        return this;
    }

    public JenkinsPipelineBuilder WithStage(string name, Action<StageBuilder> configureStage)
    {
        var stageBuilder = new StageBuilder(name);
        configureStage(stageBuilder);
        _pipeline.Node.Stages.Add(stageBuilder.Build());
        return this;
    }

    public string Build() => _pipeline.ToGroovy();

    private class Pipeline
    {
        public Node Node { get; set; }
        public List<ITrigger> Triggers { get; set; } = new List<ITrigger>();

        public string ToGroovy()
        {
            var sb = new StringBuilder();
            if (Triggers.Any())
            {
                sb.AppendLine("properties([");
                sb.AppendLine("    pipelineTriggers([");
                foreach (var trigger in Triggers)
                {
                    sb.AppendLine(trigger.ToGroovy().Indent(2));
                }
                sb.AppendLine("    ])");
                sb.AppendLine("])");
                sb.AppendLine();
            }
            sb.AppendLine(Node.ToGroovy());
            return sb.ToString();
        }
    }

    public class TriggersBuilder
    {
        private readonly List<ITrigger> _triggers = new List<ITrigger>();

        public TriggersBuilder WithCron(string schedule)
        {
            _triggers.Add(new CronTrigger(schedule));
            return this;
        }

        public TriggersBuilder WithPollSCM(string schedule)
        {
            _triggers.Add(new PollSCMTrigger(schedule));
            return this;
        }

        public TriggersBuilder WithUpstream(Action<UpstreamTriggerBuilder> configure)
        {
            var builder = new UpstreamTriggerBuilder();
            configure(builder);
            _triggers.Add(builder.Build());
            return this;
        }

        public List<ITrigger> Build() => _triggers;
    }

    private interface ITrigger { string ToGroovy(); }

    private class CronTrigger : ITrigger
    {
        public string Schedule { get; }
        public CronTrigger(string schedule) => Schedule = schedule;
        public string ToGroovy() => $"cron('{Schedule}')";
    }

    private class PollSCMTrigger : ITrigger
    {
        public string Schedule { get; }
        public PollSCMTrigger(string schedule) => Schedule = schedule;
        public string ToGroovy() => $"pollSCM('{Schedule}')";
    }

    private class UpstreamTrigger : ITrigger
    {
        public List<string> Projects { get; }
        public string Threshold { get; }
        public UpstreamTrigger(List<string> projects, string threshold) => (Projects, Threshold) = (projects, threshold);
        public string ToGroovy() => $"upstream(upstreamProjects: [{string.Join(", ", Projects.Select(p => $"'{p}'")}], threshold: hudson.model.Result.{Threshold})";
    }

    public class UpstreamTriggerBuilder
    {
        private readonly List<string> _projects = new List<string>();
        private string _threshold = "SUCCESS";
        public UpstreamTriggerBuilder WithProjects(params string[] projects) { _projects.AddRange(projects); return this; }
        public UpstreamTriggerBuilder WithThreshold(string threshold) { _threshold = threshold; return this; }
        public UpstreamTrigger Build() => new UpstreamTrigger(_projects, _threshold);
    }

    private class Node
    {
        public string Label { get; }
        public List<Stage> Stages { get; } = new List<Stage>();
        public Node(string label) => Label = label;
        public string ToGroovy() => new StringBuilder()
            .AppendLine($"node('{Label}') {{")
            .AppendJoin("", Stages.Select(s => s.ToGroovy().Indent(1)))
            .AppendLine("}").ToString();
    }

    private class Stage
    {
        public string Name { get; }
        public List<IStep> Steps { get; } = new List<IStep>();
        public List<IStep> FinallySteps { get; } = new List<IStep>();
        public Stage(string name) => Name = name;
        public string ToGroovy() => new StringBuilder()
            .AppendLine($"stage('{Name}') {{")
            .AppendLine("try {".Indent(1))
            .AppendJoin("", Steps.Select(s => s.ToGroovy().Indent(2)))
            .AppendLine("}".Indent(1))
            .AppendLine("finally {".Indent(1))
            .AppendJoin("", FinallySteps.Select(s => s.ToGroovy().Indent(2)))
            .AppendLine("}".Indent(1))
            .AppendLine("}").ToString();
    }

    public class StageBuilder
    {
        private readonly Stage _stage;
        public StageBuilder(string name) => _stage = new Stage(name);
        public StageBuilder WithStep(Action<StepBuilder> configureStep)
        {
            var stepBuilder = new StepBuilder();
            configureStep(stepBuilder);
            _stage.Steps.Add(stepBuilder.Build());
            return this;
        }
        public StageBuilder WithFinally(Action<StepBuilder> configureFinally)
        {
            var stepBuilder = new StepBuilder();
            configureFinally(stepBuilder);
            _stage.FinallySteps.Add(stepBuilder.Build());
            return this;
        }
        public Stage Build() => _stage;
    }

    public class StepBuilder
    {
        private IStep _step;
        public StepBuilder Echo(string message) { _step = new EchoStep(message); return this; }
        public StepBuilder Sh(string script, Action<ShOptions> configure = null)
        {
            var shStep = new ShStep(script);
            configure?.Invoke(shStep.Options);
            _step = shStep;
            return this;
        }
        public StepBuilder Pwsh(string script, Action<PwshOptions> configure = null)
        {
            var pwshStep = new PwshStep(script);
            configure?.Invoke(pwshStep.Options);
            _step = pwshStep;
            return this;
        }
        public StepBuilder ArchiveArtifacts(string pattern) { _step = new ArchiveArtifactsStep(pattern); return this; }
        public StepBuilder JUnit(string pattern) { _step = new JUnitStep(pattern); return this; }
        public StepBuilder XUnit(string toolType, string pattern) { _step = new XUnitStep(toolType, pattern); return this; }
        public StepBuilder NUnit(string pattern) => XUnit("NUnit", pattern);
        public StepBuilder WithCredentials(Action<CredentialBindingBuilder> configure)
        {
            var builder = new CredentialBindingBuilder();
            configure(builder);
            _step = new WithCredentialsStep(builder.Bindings);
            return this;
        }
        public IStep Build() => _step ?? throw new InvalidOperationException("No step defined");
    }

    private interface IStep { string ToGroovy(); }

    private class EchoStep : IStep
    {
        public string Message { get; }
        public EchoStep(string message) => Message = message;
        public string ToGroovy() => $"echo '{Message}'";
    }

    private class ShStep : IStep
    {
        public string Script { get; }
        public ShOptions Options { get; } = new ShOptions();
        public ShStep(string script) => Script = script;
        public string ToGroovy() => "sh " + string.Join(", ", new[]
        {
            $"script: '{Script}'",
            Options.ReturnStdout ? "returnStdout: true" : null,
            !string.IsNullOrEmpty(Options.Label) ? $"label: '{Options.Label}'" : null
        }.Where(p => p != null));
    }

    private class PwshStep : IStep
    {
        public string Script { get; }
        public PwshOptions Options { get; } = new PwshOptions();
        public PwshStep(string script) => Script = script;
        public string ToGroovy() => "pwsh " + string.Join(", ", new[]
        {
            $"script: '{Script}'",
            Options.ReturnStdout ? "returnStdout: true" : null,
            !string.IsNullOrEmpty(Options.Label) ? $"label: '{Options.Label}'" : null
        }.Where(p => p != null));
    }

    private class ArchiveArtifactsStep : IStep
    {
        public string Pattern { get; }
        public ArchiveArtifactsStep(string pattern) => Pattern = pattern;
        public string ToGroovy() => $"archiveArtifacts artifacts: '{Pattern}'";
    }

    private class JUnitStep : IStep
    {
        public string Pattern { get; }
        public JUnitStep(string pattern) => Pattern = pattern;
        public string ToGroovy() => $"junit '{Pattern}'";
    }

    private class XUnitStep : IStep
    {
        public string ToolType { get; }
        public string Pattern { get; }
        public XUnitStep(string toolType, string pattern) => (ToolType, Pattern) = (toolType, pattern);
        public string ToGroovy() => $@"xUnit([$class: '{ToolType}Parser', pattern: '{Pattern}'])";
    }

    private class WithCredentialsStep : IStep
    {
        public List<ICredentialBinding> Bindings { get; }
        public WithCredentialsStep(List<ICredentialBinding> bindings) => Bindings = bindings;
        public string ToGroovy() => new StringBuilder()
            .AppendLine($"withCredentials([{string.Join(", ", Bindings.Select(b => b.ToGroovy())}]) {{")
            .AppendJoin("\n", Bindings.Select(b => b.VariableAssignment.Indent(1)))
            .Append("\n}").ToString();
    }

    public class ShOptions { public bool ReturnStdout { get; set; } public string Label { get; set; } }
    public class PwshOptions { public bool ReturnStdout { get; set; } public string Label { get; set; } }

    public class CredentialBindingBuilder
    {
        public List<ICredentialBinding> Bindings { get; } = new List<ICredentialBinding>();
        public CredentialBindingBuilder AddUsernamePassword(string credentialId, string usernameVar, string passwordVar)
            => Add(new UsernamePasswordBinding(credentialId, usernameVar, passwordVar));
        public CredentialBindingBuilder AddSecretText(string credentialId, string variable)
            => Add(new SecretTextBinding(credentialId, variable));
        public CredentialBindingBuilder AddSshKey(string credentialId, string keyFileVar)
            => Add(new SshKeyBinding(credentialId, keyFileVar));
        private CredentialBindingBuilder Add(ICredentialBinding binding) { Bindings.Add(binding); return this; }
    }

    private interface ICredentialBinding { string ToGroovy(); string VariableAssignment { get; } }

    private class UsernamePasswordBinding : ICredentialBinding
    {
        public string CredentialId { get; }
        public string UsernameVar { get; }
        public string PasswordVar { get; }
        public UsernamePasswordBinding(string id, string userVar, string passVar) => (CredentialId, UsernameVar, PasswordVar) = (id, userVar, passVar);
        public string ToGroovy() => $"usernamePassword(credentialsId: '{CredentialId}', usernameVariable: '{UsernameVar}', passwordVariable: '{PasswordVar}')";
        public string VariableAssignment => $"echo 'Using credentials: {UsernameVar}=***'";
    }

    private class SecretTextBinding : ICredentialBinding
    {
        public string CredentialId { get; }
        public string Variable { get; }
        public SecretTextBinding(string id, string variable) => (CredentialId, Variable) = (id, variable);
        public string ToGroovy() => $"string(credentialsId: '{CredentialId}', variable: '{Variable}')";
        public string VariableAssignment => $"echo 'Secret loaded: {Variable}'";
    }

    private class SshKeyBinding : ICredentialBinding
    {
        public string CredentialId { get; }
        public string KeyFileVar { get; }
        public SshKeyBinding(string id, string keyVar) => (CredentialId, KeyFileVar) = (id, keyVar);
        public string ToGroovy() => $"sshUserPrivateKey(credentialsId: '{CredentialId}', keyFileVariable: '{KeyFileVar}')";
        public string VariableAssignment => $"echo 'SSH key file: {KeyFileVar}'";
    }
}

public class JenkinsConfigAsCodeBuilder
{
    private readonly List<Credential> _credentials = new List<Credential>();
    public JenkinsConfigAsCodeBuilder AddCredential(Action<CredentialBuilder> configure)
    {
        var builder = new CredentialBuilder();
        configure(builder);
        _credentials.Add(builder.Build());
        return this;
    }
    public string GenerateYaml() => new StringBuilder()
        .AppendLine("jenkins:")
        .AppendLine("  securityRealm: [...]")
        .AppendLine("credentials:")
        .AppendLine("  system:")
        .AppendLine("    domainCredentials:")
        .AppendLine("      - credentials:")
        .AppendJoin("", _credentials.Select(c => c.ToYaml().Indent(8)))
        .ToString();

    private abstract class Credential
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public abstract string ToYaml();
    }

    private class UsernamePasswordCredential : Credential
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public override string ToYaml() => $@"- usernamePassword:
      scope: GLOBAL
      id: {Id}
      username: {Username}
      password: {Password}
      description: ""{Description}""";
    }

    private class SecretTextCredential : Credential
    {
        public string Secret { get; set; }
        public override string ToYaml() => $@"- string:
      scope: GLOBAL
      id: {Id}
      secret: {Secret}
      description: ""{Description}""";
    }

    private class SshKeyCredential : Credential
    {
        public string PrivateKey { get; set; }
        public string Passphrase { get; set; }
        public override string ToYaml() => $@"- basicSSHUserPrivateKey:
      scope: GLOBAL
      id: {Id}
      username: ""jenkins""
      privateKeySource:
        directEntry:
          privateKey: |-
            {PrivateKey}
      passphrase: {Passphrase}
      description: ""{Description}""";
    }

    public class CredentialBuilder
    {
        private Credential _credential;
        public CredentialBuilder UsernamePassword(string id, Action<UsernamePasswordCredential> configure)
        {
            var cred = new UsernamePasswordCredential { Id = id };
            configure(cred);
            _credential = cred;
            return this;
        }
        public CredentialBuilder SecretText(string id, Action<SecretTextCredential> configure)
        {
            var cred = new SecretTextCredential { Id = id };
            configure(cred);
            _credential = cred;
            return this;
        }
        public CredentialBuilder SshKey(string id, Action<SshKeyCredential> configure)
        {
            var cred = new SshKeyCredential { Id = id };
            configure(cred);
            _credential = cred;
            return this;
        }
        public Credential Build() => _credential ?? throw new InvalidOperationException("No credential type specified");
    }
}

public static class StringExtensions
{
    public static string Indent(this string text, int indentLevel)
    {
        var indent = new string(' ', indentLevel * 4);
        return string.Join(Environment.NewLine, text.Split(new[] { Environment.NewLine }, StringSplitOptions.None)
            .Select(line => indent + line)
            .ToArray();
    }
}


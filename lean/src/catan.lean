/-
Lean 4 model of core Settlers of Catan mechanics.
Designed as an engine / rules model, not a UI.
-/

inductive Resource
| Brick | Lumber | Wool | Grain | Ore
deriving DecidableEq, Repr

abbrev Resources := Resource → Int

def emptyResources : Resources := fun _ => 0

def addResources (a b : Resources) : Resources :=
  fun r => a r + b r

def subResources (a b : Resources) : Resources :=
  fun r => a r - b r

def canAfford (have : Resources) (cost : Resources) : Bool :=
  (Resource.brick   |> fun r => have r >= cost r) &&
  (Resource.lumber  |> fun r => have r >= cost r) -- simplified; check all below
  = 
  let check := fun r => have r >= cost r
  check Resource.Brick && check Resource.Lumber && check Resource.Wool && check Resource.Grain && check Resource.Ore

-- Helper to construct resource bundles
def mkRes (b l w g o : Int) : Resources :=
  fun r =>
    match r with
    | Resource.Brick  => b
    | Resource.Lumber => l
    | Resource.Wool   => w
    | Resource.Grain  => g
    | Resource.Ore    => o

-- Standard build costs
def costRoad : Resources := mkRes 1 1 0 0 0
def costSettlement : Resources := mkRes 1 1 1 1 0
def costCity : Resources := mkRes 0 0 0 2 3
def costDevCard : Resources := mkRes 0 0 1 1 1

inductive HexType
| Field (res : Resource) -- resource-producing hex
| Desert
deriving Repr

structure Hex where
  id : Nat
  hexType : HexType
  numberToken : Option Nat -- 2..12 except 7; None for desert
deriving Repr

-- Board geometry: vertices and edges simplified as indices.
-- In full implementation these would be spatial/adjacency graphs.
abbrev Vertex := Nat
abbrev Edge := Nat

inductive Building
| None
| Settlement (owner : Nat) -- player id
| City (owner : Nat)
deriving Repr

inductive Road
| None
| Road (owner : Nat)
deriving Repr

structure Board where
  hexes : List Hex
  -- adjacency maps (vertex -> adjacent hex ids)
  vertexAdjHex : Vertex → List Nat
  vertexBuilding : Vertex → Building
  edgeRoad : Edge → Road
deriving Repr

structure Player where
  id : Nat
  name : String
  resources : Resources
  devCards : Nat -- number of development cards held (simplified)
  playedKnights : Nat
  victoryPoints : Nat
  roads : List Edge
  settlements : List Vertex
  cities : List Vertex
  longestRoadLength : Nat
deriving Repr

inductive GamePhase
| SetupFirstPlacement
| SetupSecondPlacement
| Playing
deriving Repr

structure GameState where
  board : Board
  players : Array Player
  currentPlayer : Nat -- index into players
  robberHex : Nat -- hex id where robber sits
  phase : GamePhase
  largestArmyOwner : Option Nat
  longestRoadOwner : Option Nat
deriving Repr

-- Sample helpers

def getPlayer (s : GameState) (pid : Nat) : Player :=
  s.players[pid]!

def updatePlayer (s : GameState) (pid : Nat) (f : Player → Player) : GameState :=
  { s with players := s.players.set! pid (f s.players[pid]!) }

-- Resource production on a dice roll (2..12)
def produceForRoll (s : GameState) (roll : Nat) : GameState :=
  if roll = 7 then
    -- robber triggered: no production; in real game players with >7 cards discard half
    s
  else
    let give (st : GameState) : GameState :=
      (st.players.toList.enum.foldl (fun acc (i, p) =>
        -- compute resources gained by player p from all adjacent vertices to hexes with numberToken = roll
        let gain := 
          st.board.vertexAdjHex i |>.foldl (fun resAcc hid =>
            match st.board.hexes.get? hid with
            | some hex =>
              match hex.numberToken with
              | some n => if n = roll && (st.robberHex ≠ hid) then
                            -- check if player has settlement/city on this vertex
                            match st.board.vertexBuilding i with
                            | Building.Settlement owner => if owner = p.id then addResources resAcc (mkRes (if hex.hexType = HexType.Field Resource.Brick then 1 else 0) 0 0 0 0) else resAcc
                            | Building.City owner => if owner = p.id then addResources resAcc (mkRes 2 0 0 0 0) else resAcc
                            | _ => resAcc
                          else resAcc
              | none => resAcc
            | none => resAcc
          ) emptyResources
        -- NOTE: Above resource assignment is simplified: it doesn't map hex.resource to correct Resource enum.
        -- For a real engine, map hex.hexType Field(res) to that resource and add 1 (settlement) or 2 (city).
        acc
      ) st.players.toList.foldl (fun _ _ => st) st) -- placeholder; keep original state
    s -- simplified: no change
    -- TODO: implement mapping from hex.resource to resource and updating player resources

-- Building actions

def canBuildRoad (p : Player) : Bool :=
  p.resources Resource.Brick >= 1 && p.resources Resource.Lumber >= 1

def buildRoad (s : GameState) (pid : Nat) (edge : Edge) : Option GameState :=
  let p := getPlayer s pid
  if canBuildRoad p then
    -- check edge is empty etc (omitted)
    let newRes := subResources p.resources costRoad
    let newPlayer := { p with resources := newRes, roads := edge :: p.roads }
    some (updatePlayer s pid (fun _ => newPlayer))
  else none

def canBuildSettlement (p : Player) : Bool :=
  -- simplified affordability
  p.resources Resource.Brick >= 1 && p.resources Resource.Lumber >= 1 && p.resources Resource.Wool >= 1 && p.resources Resource.Grain >= 1

def buildSettlement (s : GameState) (pid : Nat) (v : Vertex) : Option GameState :=
  let p := getPlayer s pid
  if canBuildSettlement p then
    -- check vertex empty etc (omitted)
    let newRes := subResources p.resources costSettlement
    let newPlayer := { p with resources := newRes, settlements := v :: p.settlements, victoryPoints := p.victoryPoints + 1 }
    let newBoard := { s.board with vertexBuilding := fun x => if x = v then Building.Settlement pid else s.board.vertexBuilding x }
    some { s with players := s.players.set! pid newPlayer, board := newBoard }
  else none

def upgradeToCity (s : GameState) (pid : Nat) (v : Vertex) : Option GameState :=
  let p := getPlayer s pid
  -- check player has settlement at v
  if v ∈ p.settlements && p.resources Resource.Grain >= 2 && p.resources Resource.Ore >= 3 then
    let newRes := subResources p.resources costCity
    let newPlayer := { p with resources := newRes, settlements := p.settlements.filter (· ≠ v), cities := v :: p.cities, victoryPoints := p.victoryPoints + 1 }
    let newBoard := { s.board with vertexBuilding := fun x => if x = v then Building.City pid else s.board.vertexBuilding x }
    some { s with players := s.players.set! pid newPlayer, board := newBoard }
  else none

-- Turn flow (simplified)

inductive TurnAction
| Roll (n : Nat)
| BuildRoad (edge : Edge)
| BuildSettlement (vertex : Vertex)
| BuildCity (vertex : Vertex)
| EndTurn
deriving Repr

def applyAction (s : GameState) (a : TurnAction) : Option GameState :=
  match a with
  | TurnAction.Roll n => some (produceForRoll s n)
  | TurnAction.BuildRoad e => buildRoad s s.currentPlayer e
  | TurnAction.BuildSettlement v => buildSettlement s s.currentPlayer v
  | TurnAction.BuildCity v => upgradeToCity s s.currentPlayer v
  | TurnAction.EndTurn =>
    let next := (s.currentPlayer + 1) % s.players.size
    some { s with currentPlayer := next }

-- Initialization helpers (very simplified, fixed tiny board)

def sampleHexes : List Hex :=
  [ { id := 0, hexType := HexType.Field Resource.Brick, numberToken := some 6 },
    { id := 1, hexType := HexType.Field Resource.Lumber, numberToken := some 8 },
    { id := 2, hexType := HexType.Desert, numberToken := none } ]

def sampleVertexAdj : Vertex → List Nat
| 0 => [0,1]
| 1 => [1,2]
| 2 => [0,2]
| _ => []

def emptyBoard : Board :=
  { hexes := sampleHexes, vertexAdjHex := sampleVertexAdj, vertexBuilding := fun _ => Building.None, edgeRoad := fun _ => Road.None }

def mkPlayer (id : Nat) (name : String) : Player :=
  { id := id, name := name, resources := emptyResources, devCards := 0, playedKnights := 0, victoryPoints := 0, roads := [], settlements := [], cities := [], longestRoadLength := 0 }

def initGame (names : List String) : GameState :=
  let ps := names.enum.map fun (i, n) => mkPlayer i n
  { board := emptyBoard,
    players := ps.toArray,
    currentPlayer := 0,
    robberHex := 2,
    phase := GamePhase.Playing,
    largestArmyOwner := none,
    longestRoadOwner := none }

-- Example usage (in Lean IO or tests, omitted here). Replace RNG and I/O to interact.


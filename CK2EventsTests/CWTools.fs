module CWToolsTests
open Expecto
open CK2Events.Application
open System.Linq
open FParsec
open System.IO
open Expecto.Expect
open Newtonsoft.Json
open CK2Events.Controllers.Utils
open System.Text
open CWTools.Common
open CWTools.Parser.Types
open CWTools.Process
open CWTools.Localisation
open CWTools.Localisation.CK2Localisation
open CWTools.Process.CK2Process
open CWTools.Games
open CWTools.Games.Stellaris
open CWTools
open System.Diagnostics
open CWTools.Parser.DocsParser
open CWTools.Parser
open CWTools.Games.Files
open CWTools.Utilities.Position

let winFolder = "F:\\Games\\Steam\\steamapps\\common\\"
let linuxFolder = "/home/thomas/.steam/steam/steamapps/common/"
let steamFolder = winFolder

let emptyStellarisSettings (rootDirectory) = {
    rootDirectory = rootDirectory
    scope = FilesScope.All
    modFilter = None
    validation = {
        validateVanilla = false
        experimental = true
        langs = [STL STLLang.English]
    }
    rules = None
    scriptFolders = None
    embedded = {
        triggers = []
        effects = []
        modifiers = []
        embeddedFiles = []
        cachedResourceData = []
    }
}

let test f =
    let x = CKParser.parseFile f
    match x with
    |Success(_,_,_) -> ()
    |Failure(msg,_,_) -> Expect.isTrue false (f + " " + msg)
[<Tests>]
let processTests =
    testList "process tests" [
        testCase "process artifacts" <| fun () ->
            let slots = [KeyValue(PosKeyValue(range.Zero, KeyValueItem(Key("weapon"),Int(1))));
                         KeyValue(PosKeyValue(range.Zero, KeyValueItem(Key("ceremonial_weapon"), Int(1))));
                         KeyValue(PosKeyValue(range.Zero, KeyValueItem(Key("scepter"), Int(1))));
                         KeyValue(PosKeyValue(range.Zero, KeyValueItem(Key("crown"), Int(1))));
                         KeyValue(PosKeyValue(range.Zero, KeyValueItem(Key("wrist"), Int(1))));
                         KeyValue(PosKeyValue(range.Zero, KeyValueItem(Key("neck"), Int(1))));
                         KeyValue(PosKeyValue(range.Zero,  KeyValueItem(Key("torso"), Int(1))));
                         KeyValue(PosKeyValue(range.Zero, KeyValueItem(Key("ceremonial_torso"), Int(1))));
                        ]
            let Success(parsed, _, _) as t = (CKParser.parseFile "CK2EventsTests/crusader kings 2/artifacts.txt")
            let Success(parsed2, _, _) as t2 = (CKParser.parseFile "CK2EventsTests/crusader kings 2/artifacts2.txt")
            let processed = CK2Process.processArtifact (parsed @ parsed2)
            Expect.equal processed.Slots slots "Not equal"

        testList "process all" [
            let folders = Directory.EnumerateDirectories (steamFolder + "Crusader Kings II/common") |> List.ofSeq
            let files = folders |> List.map (Directory.EnumerateFiles >> List.ofSeq) |> List.collect id |> List.filter (fun f -> Path.GetExtension(f) = ".txt")
            yield! files |> List.map (fun f -> testCase ("process one " + f.ToString()) <| fun () -> test f)
        ]

        // testCase "STLGame test" <| fun () ->
        //     let game = STLGame(steamFolder + "Stellaris")
        //     let results = game.Results
        //     results |> List.tryFind (function |FileResult.Fail(f, e) -> true |_ -> false)
        //             |> function |Some (Fail(k, e)) -> Expect.isTrue false (k + " " + e) |None -> ()

        // testCase "STLGame test2" <| fun () ->
        //     let game = STLGame(steamFolder + "Stellaris")
        //     let duplicates = game.Duplicates
        //     List.iter (fun d -> printfn "%A" d |> ignore) duplicates

        testCase "STLGame ship validation" <| fun () ->
            let game = STLGame(emptyStellarisSettings "CK2EventsTests/stellaris") :> IGame//, FilesScope.All, "", [], [], [], [], [], false, false)
            let errors = game.ValidationErrors()
            Expect.hasCountOf errors 2u (fun _ -> true) "Not enough errors"
            //errors |> List.iter (fun (s, e) -> printfn "%A" (s.ToRaw |> CKPrinter.api.prettyPrintStatements, e, s.Position) |> ignore)
        // testCase "process all CK2" <| fun () ->
        //     let commons =
        //         Directory.EnumerateDirectories "/home/thomas/.steam/steam/steamapps/common/Crusader Kings II/common"
        //         |> List.ofSeq
        //         |> List.map (Directory.EnumerateFiles >> List.ofSeq)
        //         |> List.collect id
        //         |> List.filter (fun f -> Path.GetExtension(f) = ".txt")
        //     commons |> List.map (CKParser.parseFile)
        //             |> List.iter (function |Success(parsed,_,_) -> () |Failure(msg, _, _) -> Expect.isTrue false msg )
    ]

[<Tests>]
let docsTests =
    testList "docs tests" [
        testCase "effects parse" <| fun () ->
            let path = "CK2EventsTests/game_effects (1).txt"
            let parsed = DocsParser.parseDocsFile path
            match parsed with
            |Failure(msg,_,_) -> Expect.isTrue false msg
            |_ -> ()
        testCase "triggers parse" <| fun () ->
            let path = "CK2EventsTests/game_triggers (1).txt"
            let parsed = DocsParser.parseDocsFile path
            match parsed with
            |Failure(msg,_,_) -> Expect.isTrue false msg
            |_ -> ()
    ]
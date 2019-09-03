module Program

open FileSystem
open Photos


let private invalidUsage =
    """Invalid usage
USAGE: photos-organizer <FROM-DIR> <TO-DIR>
"""


let private extractArgs argv : Result<string * string, string> =
    match argv with
    | [| arg1 ; arg2 |] -> Ok (arg1, arg2)
    | _ -> Error invalidUsage


[<EntryPoint>]
let main argv =
    let commandResult =
        result {
            let! fromArg, toArg = extractArgs argv
            let! fromPath = DirPath.existing fromArg
            let! toPath = DirPath.findOrCreate toArg

            return! Photos.organize fromPath toPath
        }

    match commandResult with
    | Ok _ ->
        printfn "\nOK\n"
        0
    | Error message ->
        eprintfn "%s\nERROR - We encountered some errors while processing the inputs" message
        1

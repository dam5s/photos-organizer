module FileSystem

open System.IO


type FilePath =
    private FilePath of string

type DirPath =
    private DirPath of string


[<RequireQualifiedAccess>]
module FilePath =
    let value (FilePath path) =
        path

    let fileName (FilePath path) =
        Seq.last (path.Split("/"))

    let move (DirPath destination) (filePath : FilePath) : Result<unit, string> =
        try
            let filePathValue = value filePath
            File.Move(filePathValue, sprintf "%s/%s" destination (fileName filePath))
            Ok ()
        with
        | ex -> Error (sprintf "Moving file %s failed with %s" (value filePath) ex.Message)



[<RequireQualifiedAccess>]
module DirPath =
    let private pathDoesNotExistError path =
        Error (sprintf "The path %s should be an existing directory" path)

    let private createDir path =
        try
            Directory.CreateDirectory(path) |> ignore
            Ok (DirPath path)
        with
        | ex ->
            Error (sprintf "There was an error creating the directory at %s - %s" path ex.Message)


    let value (DirPath path) = path

    let private findDirectoryOr (whenNotExist : string -> Result<DirPath, string>) (path : string) : Result<DirPath, string> =
        if Directory.Exists(path)
            then Ok (DirPath path)
            else if File.Exists(path)
                then Error (sprintf "Path %s is a file, it should be a directory" path)
                else whenNotExist path

    let existing (path : string) : Result<DirPath, string> =
        path |> findDirectoryOr pathDoesNotExistError

    let findOrCreate (path : string) : Result<DirPath, string> =
        path |> findDirectoryOr createDir

    let files (DirPath path) : Result<FilePath seq, string> =
        try
            ( path, "*", SearchOption.AllDirectories )
            |> Directory.GetFiles
            |> Array.toSeq
            |> Seq.map FilePath
            |> Ok
        with
        | ex ->
            Error (sprintf "Error listing files in directory %s. Got %s" path ex.Message)

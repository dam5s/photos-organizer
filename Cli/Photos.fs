module Photos

open System
open FileSystem


module Exif =
    open MetadataExtractor
    open MetadataExtractor.Formats.Exif
    open System.Globalization

    let private parseExifDateTime str =
        DateTime.ParseExact(str, "yyyy:MM:dd HH:mm:ss", CultureInfo.CurrentCulture, DateTimeStyles.None)

    let private tryFindDateTime (directory : Directory) : DateTime option =
        directory.Tags
        |> Seq.tryFind (fun tag -> tag.Name = "Date/Time Digitized")
        |> Option.map (fun tag -> parseExifDateTime tag.Description)

    let readDateTime (filePath : FilePath) : Result<DateTime, string> =
        try
            ImageMetadataReader.ReadMetadata(FilePath.value filePath)
            |> Seq.tryFind (fun f -> f :? ExifSubIfdDirectory)
            |> Option.bind tryFindDateTime
            |> Result.fromOption "Could not find EXIF date"
        with
        | ex ->
            let path = FilePath.value filePath
            Error (sprintf "Error reading EXIF date at path %s. Got: %s." path ex.Message)


[<RequireQualifiedAccess>]
module Photos =

    let private destination (baseDir : DirPath) (date : DateTime) : Result<DirPath, string> =
        let stringValue = sprintf "%s/%s" (DirPath.value baseDir) (date.ToString "yyyy/MM - MMMM")
        DirPath.findOrCreate stringValue

    let private fileError filePathValue message =
        let updatedMessage = sprintf "%s - %s" filePathValue message
        eprintfn "ERROR - %s" updatedMessage
        Error updatedMessage


    let private organizeFile (toDir : DirPath) (filePath : FilePath) : Result<unit, string> =
        let moveResult =
            result {
                let! date = Exif.readDateTime filePath
                let! destinationPath = destination toDir date
                let! _ = FilePath.move destinationPath filePath
                return destinationPath
            }

        let filePathValue = FilePath.value filePath

        match moveResult with
        | Ok destinationPath ->
            printfn "OK - %s -> %s" filePathValue (DirPath.value destinationPath)
            Ok ()
        | Error message ->
            fileError filePathValue message

    let organize (fromDir : DirPath) (toDir : DirPath) : Result<unit, string> =
        result {
            printfn "Organizing photos from %A to %A...\n" fromDir toDir

            let! files = DirPath.files fromDir

            return! files
                    |> Seq.map (organizeFile toDir)
                    |> Result.sequence
                    |> Result.map (always ())
                    |> Result.mapError (always "Some errors were encountered.")
        }

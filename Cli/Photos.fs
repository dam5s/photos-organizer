module Photos

open FileSystem
open System
open System.Text.RegularExpressions


module private Exif =
    open MetadataExtractor
    open MetadataExtractor.Formats.Exif
    open System.Globalization

    let private parseExifDateTime str =
        DateTime.ParseExact(str, "yyyy:MM:dd HH:mm:ss", CultureInfo.CurrentCulture, DateTimeStyles.None)

    let private tryFindDateTime (directory: Directory): DateTime option =
        directory.Tags
        |> Seq.tryFind (fun tag -> tag.Name = "Date/Time Digitized")
        |> Option.map (fun tag -> parseExifDateTime tag.Description)

    let readDateTime (filePath: FilePath): Result<DateTime, string> =
        try
            ImageMetadataReader.ReadMetadata(FilePath.value filePath)
            |> Seq.tryFind (fun f -> f :? ExifSubIfdDirectory)
            |> Option.bind tryFindDateTime
            |> Result.fromOption "Could not find EXIF date"
        with
        | ex ->
            let path = FilePath.value filePath
            Error(sprintf "Error reading EXIF date at path %s. Got: %s." path ex.Message)


[<RequireQualifiedAccess>]
module Photos =

    let private readDateTimeFromFilename (filePath: FilePath): Result<DateTime, string> =
        let fileName = FilePath.fileName filePath
        let matches = Regex.Match(fileName, "(\d{4})-(\d{2})-(\d{2})")
        let groups = Seq.toList matches.Groups

        match groups with
        | [ _; year; month; day ] ->
            (int year.Value, int month.Value, int day.Value)
            |> DateTime
            |> Ok
        | _ -> Error "Could not extract date from file"

    let private extractDate (filePath: FilePath): Result<DateTime, string> =
        match readDateTimeFromFilename filePath with
        | Error _ -> Exif.readDateTime filePath
        | ok -> ok

    let private destination (baseDir: DirPath) (date: DateTime): Result<DirPath, string> =
        [ DirPath.value baseDir; date.ToString "yyyy"; date.ToString "MM-MMMM" ]
        |> Path.join
        |> DirPath.findOrCreate

    let private fileError filePathValue message =
        let updatedMessage = sprintf "%s - %s" filePathValue message
        eprintfn "ERROR - %s" updatedMessage
        Error updatedMessage

    let private organizeFile (toDir: DirPath) (filePath: FilePath): Result<unit, string> =
        let moveResult =
            result {
                let! date = extractDate filePath
                let! destinationPath = destination toDir date
                let! _ = FilePath.move destinationPath filePath
                return destinationPath
            }

        let filePathValue = FilePath.value filePath

        match moveResult with
        | Ok destinationPath ->
            printfn "OK - %s -> %s" filePathValue (DirPath.value destinationPath)
            Ok()
        | Error message ->
            fileError filePathValue message

    let organize (fromDir: DirPath) (toDir: DirPath): Result<unit, string> =
        result {
            printfn "Organizing photos from %s to %s...\n" (DirPath.value fromDir) (DirPath.value toDir)

            let! files = DirPath.files fromDir

            return! files
                    |> Seq.map (organizeFile toDir)
                    |> Result.sequence
                    |> Result.map (always())
                    |> Result.mapError (always "Some errors were encountered.")
        }

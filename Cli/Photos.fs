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

    let organize (fromDir : DirPath) (toDir : DirPath) : Result<unit, string> =
        result {
            printfn "Organizing photos from %A to %A..." fromDir toDir

            let! files = DirPath.files fromDir

            files
            |> Seq.iter (fun file ->
                match Exif.readDateTime file with
                | Ok date -> printfn "%A has date %A" file date
                | Error msg -> eprintfn "%A has error %A" file msg
            )

            return ()
        }

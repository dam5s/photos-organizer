module Photos

open FileSystem


[<RequireQualifiedAccess>]
module Photos =

    let organize (fromDir : DirPath) (toDir : DirPath) : Result<unit, string> =
        printfn "Organizing photos from %s to %s... not." (DirPath.value fromDir) (DirPath.value toDir)
        Ok ()

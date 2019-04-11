[<AutoOpen>]
module Prelude


let always a _ = a


[<RequireQualifiedAccess>]
module Seq =
    let any predicate sequence : bool =
        match Seq.tryFind predicate sequence with
        | Some _ -> true
        | None -> false


[<RequireQualifiedAccess>]
module Result =
    let fromOption onError opt =
        match opt with
        | Some x -> Ok x
        | None -> Error onError

    let private appendError (newErr : 'TErr) (result : Result<'T, 'TErr seq>) =
        let newErrSeq = Seq.singleton newErr

        match result with
        | Ok _ -> Error newErrSeq
        | Error msgs -> Error (Seq.append msgs newErrSeq)

    let sequence (results : Result<'T, 'TErr> seq) : Result<'T seq, 'TErr seq> =
        (Ok Seq.empty, results)
        ||> Seq.fold (fun overallResult elementResult ->
            match elementResult with
            | Ok element -> Result.map (fun elements -> Seq.append elements (Seq.singleton element)) overallResult
            | Error msg -> appendError msg overallResult
        )


type ResultBuilder() =
    member x.Bind(v,f) = Result.bind f v
    member x.Return v = Ok v
    member x.ReturnFrom o = o
    member x.Zero () = Error "error"

let result = ResultBuilder()

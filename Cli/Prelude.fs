[<AutoOpen>]
module Prelude


type SimpleResult<'T> =
    Result<'T, string>

type SimpleResultBuilder() =
    member x.Bind(v,f) = Result.bind f v
    member x.Return v = Ok v
    member x.ReturnFrom o = o
    member x.Zero () = Error "error"

let result = SimpleResultBuilder()

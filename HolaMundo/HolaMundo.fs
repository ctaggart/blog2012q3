namespace Hola
module Mundo =
    open IntelliFactory.WebSharper
    open IntelliFactory.WebSharper.JavaScript

    [<JavaScript>]
    let rec Factorial n =
        match n with
        | 0 -> 1
        | n -> n * Factorial (n - 1)

    [<JavaScript>]
    let AlertFactorial n =
        let fact = Factorial n
        "Factorial(" + n.ToString() + ") = " + fact.ToString() |> Alert
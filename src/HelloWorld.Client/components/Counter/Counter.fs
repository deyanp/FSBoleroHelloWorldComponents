module HelloWorld.Client.Components.Counter

open System
open Elmish
open Bolero
open Bolero.Html
open Bolero.Json
open Bolero.Remoting
open Bolero.Remoting.Client
open Bolero.Templating.Client
open System.Net.Http
open Microsoft.AspNetCore.Components
open HelloWorld.Client.Services
open System.Net.Http

type Model =
    {
        Counter: int
    }

type Message =
    | Increment
    | Decrement
    | SetCounter of int

let init =
    { Counter = 0}

let update message model =
    match message with
    | Increment ->
        { model with Counter = model.Counter + 1 }, Cmd.none
    | Decrement ->
        { model with Counter = model.Counter - 1 }, Cmd.none
    | SetCounter value ->
        { model with Counter = value }, Cmd.none

type ViewTemplate = Template<"components/Counter/counter.html">

let view model dispatch =
    ViewTemplate()
        .Decrement(fun _ -> dispatch Decrement)
        .Increment(fun _ -> dispatch Increment)
        .Value(model.Counter, fun v -> dispatch (SetCounter v))
        .Elt()




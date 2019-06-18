module HelloWorld.Client.Components.Home

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
        Nothing: string option
    }

type Message =
    | Nothing

let init =
    { Nothing = None}

type ViewTemplate = Template<"components/Home/home.html">

let view model dispatch =
    ViewTemplate().Elt() 




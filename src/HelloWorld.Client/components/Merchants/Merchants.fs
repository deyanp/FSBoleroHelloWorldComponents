module HelloWorld.Client.Components.Merchants

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
        Merchants: MerchantService.Merchant list option
    }

type Message =
    | GetMerchants
    | GotMerchants of MerchantService.Merchant list
    | Error of exn

let init =
    { Merchants = None}

let update (httpClient:HttpClient) message model =
    match message with
    | GetMerchants ->
        let cmd = Cmd.ofAsync MerchantService.getMerchants httpClient GotMerchants Error
        { model with Merchants = None }, cmd
    | GotMerchants merchants ->
        { model with Merchants = Some merchants }, Cmd.none

type ViewTemplate = Template<"components/Merchants/merchants.html">

let view model dispatch =
    ViewTemplate()
        .Reload(fun _ -> dispatch GetMerchants)
        .Rows(cond model.Merchants <| function
            | None ->
                ViewTemplate.EmptyData().Elt()
            | Some merchants ->
                forEach merchants <| fun merchant ->
                    tr [] [
                        td [] [text merchant.Id]
                        td [] [text merchant.Name]
                    ])
        .Elt()        




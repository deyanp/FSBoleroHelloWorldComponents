module HelloWorld.Client.Components.SignIn

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
        Username: string
        Password: string
        SignedInAs: option<string>
        SignInFailed: bool
        Error: string option
    }

type Message =
    | SetUsername of string
    | SetPassword of string
    | GetSignedInAs
    | RecvSignedInAs of RemoteResponse<string>
    | SendSignIn
    | RecvSignIn of option<string>
    | SendSignOut
    | RecvSignOut
    | Error of exn

type ExternalMsg =
    | NoOp
    | RaiseError of string
    | SignedIn

let init =
    { 
        Username = ""
        Password = ""
        SignedInAs = None
        SignInFailed = false
        Error = None
    }

/// Remote service definition.
type IAMService =
    {
        /// Sign into the application.
        signIn : string * string -> Async<option<string>>

        /// Get the user's name, or None if they are not authenticated.
        getUsername : unit -> Async<string>

        /// Sign out from the application.
        signOut : unit -> Async<unit>
    }    
    interface IRemoteService with
        member this.BasePath = "/iam"    

let update iamService message model =
    // let onSignIn = function
    //     | Some _ -> Cmd.ofMsg GetBooks
    //     | None -> Cmd.none

    match message with
    | SetUsername s ->
        { model with Username = s }, Cmd.none, NoOp
    | SetPassword s ->
        { model with Password = s }, Cmd.none, NoOp
    | GetSignedInAs ->
        model, Cmd.ofRemote iamService.getUsername () RecvSignedInAs Error, NoOp
    | RecvSignedInAs resp ->
        let username = resp.TryGetResponse()
        // { model with SignedInAs = username }, onSignIn username, NoOp
        { model with SignedInAs = username }, Cmd.none, NoOp
    | SendSignIn ->
        model, Cmd.ofAsync iamService.signIn (model.Username, model.Password) RecvSignIn Error, NoOp
    | RecvSignIn username ->
        // { model with SignedInAs = username; SignInFailed = Option.isNone username }, onSignIn username, 
        { model with SignedInAs = username; SignInFailed = Option.isNone username }, Cmd.none, 
            match username with 
            | Some _ -> SignedIn
            | None -> RaiseError "login failed"
    | SendSignOut ->
        model, Cmd.ofAsync iamService.signOut () (fun () -> RecvSignOut) Error, NoOp
    | RecvSignOut ->
        { model with SignedInAs = None; SignInFailed = false }, Cmd.none, NoOp
    | Error RemoteUnauthorizedException ->
        { model with Error = Some "You have been logged out."; SignedInAs = None }, Cmd.none, NoOp
    | Error exn ->
        { model with Error = Some exn.Message }, Cmd.none, NoOp

type ViewTemplate = Template<"components/SignIn/signin.html">

let view model dispatch =
    ViewTemplate()
        .Username(model.Username, fun s -> dispatch (SetUsername s))
        .Password(model.Password, fun s -> dispatch (SetPassword s))
        .SignIn(fun _ -> dispatch SendSignIn)
        .Elt()




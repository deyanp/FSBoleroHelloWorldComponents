module HelloWorld.Client.Main

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

/// Routing endpoints definition.
type Page =
    | [<EndPoint "/">] Home
    | [<EndPoint "/counter">] Counter
    | [<EndPoint "/books">] Books
    | [<EndPoint "/merchants">] Merchants

/// The Elmish application's model.
type Model =
    {
        page: Page
        error: string option
        //delegate to Counter component
        counterModel: HelloWorld.Client.Components.Counter.Model
        //delegate to Books component
        booksModel: HelloWorld.Client.Components.Books.Model
        //delegate to SignIn component
        signInModel: HelloWorld.Client.Components.SignIn.Model
        //delegate to Home component
        homeModel: HelloWorld.Client.Components.Home.Model
        //delegate to Merchants component
        merchantsModel: HelloWorld.Client.Components.Merchants.Model
    }

let initModel =
    {
        page = Home
        error = None
        //delegate to Home component
        counterModel = HelloWorld.Client.Components.Counter.init
        //delegate to Books component
        booksModel = HelloWorld.Client.Components.Books.init
        //delegate to Home component
        signInModel = HelloWorld.Client.Components.SignIn.init
        //delegate to Home component
        homeModel = HelloWorld.Client.Components.Home.init
        //delegate to Merchants component
        merchantsModel = HelloWorld.Client.Components.Merchants.init
    }

/// The Elmish application's update messages.
type Message =
    | SetPage of Page
    | Error of string
    | ClearError
    //delegate to Home component
    | HomeMsg of HelloWorld.Client.Components.Home.Message
    //delegate to Counter component
    | CounterMsg of HelloWorld.Client.Components.Counter.Message
    //delegate to Books component
    | BooksMsg of HelloWorld.Client.Components.Books.Message
    //delegate to SignIn component
    | SignInMsg of HelloWorld.Client.Components.SignIn.Message
    //delegate to Merchants component
    | MerchantsMsg of HelloWorld.Client.Components.Merchants.Message

let update bookService iamService (httpClient:HttpClient) message model =
    match message with
    | SetPage page ->
        { model with page = page }, Cmd.none
    //delegate to Counter component
    | CounterMsg msg ->
        let m,c = HelloWorld.Client.Components.Counter.update msg model.counterModel
        { model with counterModel = m}, Cmd.map CounterMsg c
    //delegate to Books component
    | BooksMsg msg ->
        let m,c = HelloWorld.Client.Components.Books.update bookService msg model.booksModel
        { model with booksModel = m}, Cmd.map BooksMsg c
    //delegate to SignIn component
    | SignInMsg msg ->
        let m,c,ec = HelloWorld.Client.Components.SignIn.update iamService msg model.signInModel
        let newModel, cmd = 
            match ec with
                | HelloWorld.Client.Components.SignIn.ExternalMsg.NoOp -> 
                    model, Cmd.none
                | HelloWorld.Client.Components.SignIn.ExternalMsg.RaiseError err -> 
                    { model with error = Some err }, Cmd.ofMsg (Error err)  //TODO: Check if we need both setting model and a command??
                | HelloWorld.Client.Components.SignIn.ExternalMsg.SignedIn -> 
                    model, Cmd.ofMsg (SetPage Page.Home)
        { newModel with signInModel = m}, Cmd.batch [Cmd.map SignInMsg c; cmd]
    | Error err ->
        { model with error = Some err }, Cmd.none
    | ClearError ->
        { model with error = None }, Cmd.none
    //delegate to Merchants component
    | MerchantsMsg msg ->
        let m,c = HelloWorld.Client.Components.Merchants.update httpClient msg model.merchantsModel
        { model with merchantsModel = m}, Cmd.map MerchantsMsg c

/// Connects the routing system to the Elmish application.
let router = Router.infer SetPage (fun model -> model.page)

type Main = Template<"wwwroot/main.html">

// let homePage model dispatch =
//     Main.Home().Elt()

// let counterPage model dispatch =
//     Main.Counter()
//         .Decrement(fun _ -> dispatch Decrement)
//         .Increment(fun _ -> dispatch Increment)
//         .Value(model.counter, fun v -> dispatch (SetCounter v))
//         .Elt()

// let dataPage model (username: string) dispatch =
//     Main.Data()
//         .Reload(fun _ -> dispatch GetBooks)
//         .Username(username)
//         .SignOut(fun _ -> dispatch SendSignOut)
//         .Rows(cond model.books <| function
//             | None ->
//                 Main.EmptyData().Elt()
//             | Some books ->
//                 forEach books <| fun book ->
//                     tr [] [
//                         td [] [text book.title]
//                         td [] [text book.author]
//                         td [] [text (book.publishDate.ToString("yyyy-MM-dd"))]
//                         td [] [text book.isbn]
//                     ])
//         .Elt()

// let signIinPage model dispatch =
//     Main.SignIn()
//         .Username(model.username, fun s -> dispatch (SetUsername s))
//         .Password(model.password, fun s -> dispatch (SetPassword s))
//         .SignIn(fun _ -> dispatch SendSignIn)
//         .ErrorNotification(
//             cond model.signInFailed <| function
//             | false -> empty
//             | true ->
//                 Main.ErrorNotification()
//                     .HideClass("is-hidden")
//                     .Text("Sign in failed. Use any username and the password \"password\".")
//                     .Elt()
//         )
//         .Elt()

let menuItem (model: Model) (page: Page) (text: string) =
    Main.MenuItem()
        .Active(if model.page = page then "is-active" else "")
        .Url(router.Link page)
        .Text(text)
        .Elt()

let view model dispatch =
    Main()
        .Username(model.signInModel.Username)
        .SignOut(fun _ -> dispatch (HelloWorld.Client.Components.SignIn.Message.SendSignOut |> SignInMsg))
        .Menu(concat [
            menuItem model Home "Home"
            menuItem model Counter "Counter"
            menuItem model Books "Books"
            menuItem model Merchants "Merchants"
        ])
        .Body(
            cond model.page <| function
            // | Home -> homePage model dispatch
            | Home -> 
                //delegate to Home component
                HelloWorld.Client.Components.Home.view model.homeModel (HomeMsg >> dispatch)          
            | Counter -> 
                //delegate to Counter component
                HelloWorld.Client.Components.Counter.view model.counterModel (CounterMsg >> dispatch)          
            | Books ->
                cond model.signInModel.SignedInAs <| function
                | Some username -> 
                    //delegate to Books component
                    HelloWorld.Client.Components.Books.view model.booksModel (BooksMsg >> dispatch)          
                    // dataPage model username dispatch
                | None -> 
                    //delegate to SignIn component
                    HelloWorld.Client.Components.SignIn.view model.signInModel (SignInMsg >> dispatch)          
                    // signIinPage model dispatch
            | Merchants -> 
                //delegate to Merchants component
                HelloWorld.Client.Components.Merchants.view model.merchantsModel (MerchantsMsg >> dispatch)          
        )
        .Error(
            cond model.error <| function
            | None -> empty
            | Some err ->
                Main.ErrorNotification()
                    .Text(err)
                    .Hide(fun _ -> dispatch ClearError)
                    .Elt()
        )
        .Elt()

type MyApp() =
    inherit ProgramComponent<Model, Message>()

    [<Inject>]
    member val Http = Unchecked.defaultof<HttpClient> with get, set    

    override this.Program =
        //dependency injection
        let bookService = this.Remote<HelloWorld.Client.Components.Books.BookService>()
        let iamService = this.Remote<HelloWorld.Client.Components.SignIn.IAMService>()
        let update = update bookService iamService this.Http

        // Program.mkProgram (fun _ -> initModel, Cmd.ofMsg (HelloWorld.Client.Components.SignIn.GetSignedInAs |> SignInMsg)) update view
        Program.mkProgram (fun _ -> initModel, Cmd.none) update view
        |> Program.withRouter router
#if DEBUG
        |> Program.withHotReloading
#endif

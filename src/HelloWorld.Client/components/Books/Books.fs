module HelloWorld.Client.Components.Books

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
        Books: Book[] option
    }
and Book =
    {
        title: string
        author: string
        [<DateTimeFormat "yyyy-MM-dd">]
        publishDate: DateTime
        isbn: string
    }

type Message =
    | GetBooks
    | GotBooks of Book[]
    | Error of exn

let init =
    { Books = None}

/// Remote service definition.
type BookService =
    {
        /// Get the list of all books in the collection.
        getBooks: unit -> Async<Book[]>

        /// Add a book in the collection.
        addBook: Book -> Async<unit>

        /// Remove a book from the collection, identified by its ISBN.
        removeBookByIsbn: string -> Async<unit>
    }

    interface IRemoteService with
        member this.BasePath = "/books"    

let update (remote:BookService) message model =
    match message with
    | GetBooks ->
        let cmd = Cmd.ofAsync remote.getBooks () GotBooks Error
        { model with Books = None }, cmd
    | GotBooks books ->
        { model with Books = Some books }, Cmd.none


type ViewTemplate = Template<"components/Books/books.html">

let view model dispatch =
    ViewTemplate()
        .Reload(fun _ -> dispatch GetBooks)
        .Rows(cond model.Books <| function
            | None ->
                ViewTemplate.EmptyData().Elt()
            | Some books ->
                forEach books <| fun book ->
                    tr [] [
                        td [] [text book.title]
                        td [] [text book.author]
                        td [] [text (book.publishDate.ToString("yyyy-MM-dd"))]
                        td [] [text book.isbn]
                    ])
        .Elt()




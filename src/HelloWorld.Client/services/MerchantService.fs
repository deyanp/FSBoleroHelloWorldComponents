[<RequireQualifiedAccess>]
module HelloWorld.Client.Services.MerchantService

open System
open System.Net.Http

type Merchant = 
    {
        Id : string
        Name : string        
    }    

module Merchant = 
    let fromHttp response :Merchant list= 
        []

let getMerchants (httpClient:HttpClient) :Async<Merchant list> = 
    async {
        let! response = httpClient.GetAsync("http://example.com/") |> Async.AwaitTask
        return Merchant.fromHttp response
    }
    
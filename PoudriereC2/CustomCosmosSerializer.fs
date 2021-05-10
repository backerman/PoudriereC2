namespace Facefault.PoudriereC2

open System.IO
open System.Text.Json
open Azure.Cosmos.Serialization
open System

type MyCustomCosmosSerializer() =
    inherit CosmosSerializer()
    let options = eventSerializationOptions
    override this.FromStream(stream: Stream): 'T = 
        try
            let task = JsonSerializer.DeserializeAsync<'T>(stream, options)
            Async.RunSynchronously (task.AsAsync())
        finally
            stream.Dispose()
            
    override this.ToStream(input: 'T): Stream =
        let out = JsonSerializer.SerializeToUtf8Bytes<'T>(input, options)
        let outStream = new MemoryStream(out.Length)
        outStream.Write(ReadOnlySpan<byte>(out))
        outStream.Seek(int64(0), SeekOrigin.Begin) |> ignore
        outStream :> Stream

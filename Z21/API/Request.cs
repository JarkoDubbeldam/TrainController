namespace Z21.API {
  public abstract class Request {
    internal abstract byte[] ToByteArray();
  }
  public abstract class RequestWithResponse<TResponse> : Request {
    internal abstract ResponseFactory<TResponse> GetResponseFactory();

  }
  public abstract class AddressSpecificRequest<TResponse> : RequestWithResponse<TResponse> {
  }
}

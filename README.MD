# Highwind

## Description

Highwind is a dot.net core web api for issuing Java Web Tokens (jwt) in a windows domain.

The api utilises integrated windows authentication to authenticate incoming requests and returns a jwt for use in other applications.

This allows a non windows app (i.e. linux docker container with web app) to effectively authenticate a user seemlessly with integrated windows auth.

Swagger is available at '/swagger'.

This is a proof of concept project.

## Getting Started

### A. Restore Nuget packages.
```bash
dotnet restore
```

### B. Generate a public and private key (for RSA signed JWTs - otherwise check appsettings hmacSecretKey)

1. Generate keys
```bash
openssl genrsa -out private.pem 2048 
openssl rsa -in private.pem -outform PEM -pubout -out public.pem
```
2. Convert these to XML format using a site like [RSA Key Converter](https://superdry.apphb.com/tools/online-rsa-key-converter). This is required for generating an instance of `RSA` - see `XmlHelper.cs`.
3. Place the results for the public and private keys into `public.key.xml` and `private.key.xml` in the root directory.
4. You can use [Xml Formatter](http://www.webtoolkitonline.com/xml-formatter.html)

### C. Configure `appsettings.json` as per environment.

Make sure to edit the "Cors:allowedOrigins" section to include the domain of your client app/s.

### D. Execute/Debug

```bash
dotnet run
```

## Deployment

### A. Publish

```bash
dotnet publish
```

### B. Execute
```bash
dotnet run highwind.dll
```

## Endpoints

Current version is 1.0.

### GET /v{version}/Token/auth?redirectURL

The main endpoint. 

All clients need to hit this endpoint to authenticate a user via a GET redirect or GET ajax request.

This endpoint uses integrated windows authenticatoin to authenticate the incoming request and redirects to `redirectURL` following success or failure.

Upon success, it sets an http only cookie via a Set-Cookie header in the response.

If there is a failure, it will set the cookie to an invalid value which the client will need to handle.

#### Normal workflow
1. Client receives request from user - say access root / of website.
2. Client redirects to `highwind/v{version}/Token/auth?apiKey&redirectURL`
3. Highwind auths user and sets cookie in response and redirects back to `redirectURL`
4. Client handles cookie in middleware
5. See Highwind Test Client project for details. See also `EXAMPLE.MD`.

### POST /v{version}/Token/validate

A simple validation endpoint to test a jwt is valid by highwind's perspective.

Expects Body
{
    "token": "token-value",
    "audience": "audience-value"
}


## TODO
- Add client secret Handling - i.e. need DB with client id, client secret hash + audience.
- Mapping of client secret to audience - see previous point.

## Contributing

Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

## References

- [JWT RSA & HMAC + ASP.NET Core](https://piotrgankiewicz.com/2017/07/24/jwt-rsa-hmac-asp-net-core/)
- [JWT](https://jwt.io/introduction/)

## Author

MaiorSi

## License
[MIT](https://choosealicense.com/licenses/mit/)
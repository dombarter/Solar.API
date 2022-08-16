# solar.vue

## OpenAPI Generator

1. Go to API project

```
dotnet new tool-manifest
dotnet tool install Swashbuckle.AspNetCore.Cli
```

2. Add the following items to the .csproj file

```xml
<Target Name="OpenAPI" AfterTargets="Build">
    <Exec Command="dotnet tool restore" WorkingDirectory="$(ProjectDir)" />
    <Exec Command="dotnet swagger tofile --output ../Solar.Vue/references/swagger.json $(OutputPath)$(AssemblyName).dll v1" WorkingDirectory="$(ProjectDir)" />
</Target>
```

3. This will create `swagger.json` in the Vue project under `references` folder.

4. Install OpenAPI tools

```
npm i @openapitools/openapi-generator-cli -D
```

5. Install Java

```
choco install oraclejdk
```

6. Add the following script to `package.json`:

```json
"generate-api-client": "openapi-generator-cli generate -i ./references/swagger.json -g typescript-axios -o ./src/api/"
```

https://www.npmjs.com/package/@openapitools/openapi-generator-cli

https://khalidabuhakmeh.com/generate-aspnet-core-openapi-spec-at-build-time

https://chrlschn.medium.com/net-6-web-apis-with-openapi-typescript-client-generation-a743e7f8e4f5

https://stackoverflow.com/questions/33283071/swagger-webapi-create-json-on-build

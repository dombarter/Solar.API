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

### References

https://www.npmjs.com/package/@openapitools/openapi-generator-cli

https://khalidabuhakmeh.com/generate-aspnet-core-openapi-spec-at-build-time

https://chrlschn.medium.com/net-6-web-apis-with-openapi-typescript-client-generation-a743e7f8e4f5

https://stackoverflow.com/questions/33283071/swagger-webapi-create-json-on-build

## Environment Variables

Environment variables are stored in `.env.development` and `.env.production`.
https://cli.vuejs.org/guide/mode-and-env.html#environment-variables

## Pinia User Store

Install pinia using (https://pinia.vuejs.org/getting-started.html#installation):

```
npm i pinia
```

Add these lines to `main.ts`:

```ts
import Vue from "vue";
import { createPinia, PiniaVuePlugin } from "pinia"; // <- this line!
import App from "./App.vue";
import router from "./router";

Vue.config.productionTip = false;

Vue.use(PiniaVuePlugin); // <- and this!
const pinia = createPinia(); // <- and this!

new Vue({
  router,
  pinia, // <- and finally this!
  render: (h) => h(App),
}).$mount("#app");
```

Create a user store for keeping the user information and access token (+ persisted)

```ts
import { LoginResultDto } from "@/api";
import { defineStore } from "pinia";

export const useUserStore = defineStore("user", {
  state() {
    const localUserInformation: LoginResultDto = JSON.parse(
      window.localStorage.getItem("user") || ""
    );

    return {
      email: localUserInformation.email as string | undefined | null,
      roles: localUserInformation.roles as string[] | undefined | null,
      accessToken: localUserInformation.accessToken as
        | string
        | undefined
        | null,
    };
  },
  actions: {
    login(loginResult: LoginResultDto): void {
      this.email = loginResult.email;
      this.roles = loginResult.roles;
      this.accessToken = loginResult.accessToken;

      // Persist the user information
      window.localStorage.setItem(
        "user",
        JSON.stringify({
          email: this.email,
          roles: this.roles,
          accessToken: this.accessToken,
        } as LoginResultDto)
      );
    },
    logout(): void {
      this.email = undefined;
      this.roles = undefined;
      this.accessToken = undefined;

      window.localStorage.removeItem("user");
    },
  },
});
```

This can be consumed like so:

```ts
async mounted() {
  // Login
  if (this.user.accessToken) {
    console.log("User already has access token", this.user.email);
  } else {
    const result = await this.accountService.userLoginPost({
      email: "dom.barter@3squared.com",
      password: "PA55word#",
    });
    this.user.login(result.data);
    console.log("Logged in", this.user.email, this.user.accessToken);

    // Refresh the moonService
    this.moonService = getMoonService(this.user.accessToken);
  }

  // Fetch moons
  console.log("Fetching moons");
  const moons = await this.moonService.moonsTwoGet();
  console.log("Moons fetched", moons);
},
setup() {
  // Get the user store
  const user = useUserStore();

  // Setup the services we need
  const accountService = getAccountService(user.accessToken);
  const moonService = getMoonService(user.accessToken);

  return { user, accountService, moonService };
},
```

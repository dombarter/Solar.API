<template>
  <div class="home">
    <img alt="Vue logo" src="../assets/logo.png" />
    <HelloWorld msg="Welcome to Your Vue.js + TypeScript App" />
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import HelloWorld from "@/components/HelloWorld.vue"; // @ is an alias to /src
import { AccountApi, Configuration, MoonApi } from "@/api";

export default Vue.extend({
  name: "HomeView",
  components: {
    HelloWorld,
  },
  async mounted() {
    const accountApi = new AccountApi(undefined, "https://localhost:7234");
    const result = await accountApi.userLoginPost({
      email: "dom.barter@3squared.com",
      password: "PA55word#",
    });
    console.log(result.data);

    window.localStorage.setItem("apiKey", `Bearer ${result.data}`);

    const moonApi = new MoonApi(
      new Configuration({ apiKey: window.localStorage.getItem("apiKey")! }),
      "https://localhost:7234"
    );

    const moons = await moonApi.moonsTwoGet();
    console.log(moons.data);
  },
});
</script>

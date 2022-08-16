<template>
  <div class="home">
    <img alt="Vue logo" src="../assets/logo.png" />
    <HelloWorld msg="Welcome to Your Vue.js + TypeScript App" />
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import HelloWorld from "@/components/HelloWorld.vue";
import { useUserStore } from "@/stores/user";
import { getAccountService, getMoonService } from "@/services/api";

export default Vue.extend({
  name: "HomeView",
  components: {
    HelloWorld,
  },
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
});
</script>

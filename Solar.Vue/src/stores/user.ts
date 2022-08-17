import { LoginResultDto } from "@/api";
import { defineStore } from "pinia";

export const useUserStore = defineStore("user", {
  state() {
    const localUserInformation: LoginResultDto = JSON.parse(
      window.localStorage.getItem("user") || "{}"
    );

    return {
      email: localUserInformation.email as string | undefined | null,
      roles: localUserInformation.roles as string[] | undefined | null,
    };
  },
  actions: {
    login(loginResult: LoginResultDto): void {
      this.email = loginResult.email;
      this.roles = loginResult.roles;

      // Persist the user information
      window.localStorage.setItem(
        "user",
        JSON.stringify({
          email: this.email,
          roles: this.roles,
        } as LoginResultDto)
      );
    },
    logout(): void {
      this.email = undefined;
      this.roles = undefined;

      window.localStorage.removeItem("user");
    },
  },
});

import { AccountApi, Configuration, MoonApi } from "@/api";

export function getAccountService(
  accessToken: string | undefined | null
): AccountApi {
  // Generate the account service
  const accountService = new AccountApi(
    new Configuration({
      accessToken: accessToken || "",
    }),
    process.env.VUE_APP_API_BASE_URL
  );

  return accountService;
}

export function getMoonService(
  accessToken: string | undefined | null
): MoonApi {
  // Generate the account service
  const moonService = new MoonApi(
    new Configuration({
      accessToken: accessToken || "",
    }),
    process.env.VUE_APP_API_BASE_URL
  );

  return moonService;
}

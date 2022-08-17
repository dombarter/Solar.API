import axios from "axios";
import { AccountApi, MoonApi } from "@/api";

const axiosInstance = axios.create({
  withCredentials: true,
});

export function getAccountService(): AccountApi {
  const accountService = new AccountApi(
    undefined,
    process.env.VUE_APP_API_BASE_URL,
    axiosInstance
  );
  return accountService;
}

export function getMoonService(): MoonApi {
  const moonService = new MoonApi(
    undefined,
    process.env.VUE_APP_API_BASE_URL,
    axiosInstance
  );
  return moonService;
}

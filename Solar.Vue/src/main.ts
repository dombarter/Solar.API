import Vue from "vue";
import { createPinia, PiniaVuePlugin } from "pinia";
import App from "./App.vue";
import router from "./router";

Vue.config.productionTip = false;

Vue.use(PiniaVuePlugin);
const pinia = createPinia();

new Vue({
  router,
  pinia,
  render: (h) => h(App),
}).$mount("#app");

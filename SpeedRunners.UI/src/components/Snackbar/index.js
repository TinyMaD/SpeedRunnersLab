import Vue from "vue";
import Index from "./index.vue";

let SnackbarConstructor = Vue.extend(Index);

let instance;

const Snackbar = function(options = {}) { // 就改了这里，加了个 options 参数
  instance = new SnackbarConstructor({
    data: options // 这里的 data 会传到 Index.vue 组件中的 data 中，当然也可以写在 props 里
  });
  document.getElementById('global').appendChild(instance.$mount().$el);
};
["success", "error", "info"].forEach(type => {
    Snackbar[type] = options => {
      options.color = type;
      return Snackbar(options);
    };
  });
export default Snackbar;
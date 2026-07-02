export function dispatchPrerenderEvent(vm) {
  const fire = () => {
    if (typeof document !== "undefined") {
      document.dispatchEvent(new Event("render-event"));
    }
  };
  if (vm && vm.$nextTick) {
    vm.$nextTick(fire);
  } else {
    setTimeout(fire, 0);
  }
}
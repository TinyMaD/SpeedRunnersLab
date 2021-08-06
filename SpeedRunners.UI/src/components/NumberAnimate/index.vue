<template>
  <div id="numberRun" />
</template>
<script>
import $ from "jquery";
import "./numberAnimate";
export default {
  props: {
    num: {
      type: Number,
      default: 0
    }
  },
  data() {
    return {
      numberRun: {}
    };
  },
  watch: {
    num() {
      this.numberRun.resetData(this.num);
    }
  },
  mounted() {
    this.numberRun = $("#numberRun").numberAnimate({
      num: 0,
      speed: 1500,
      symbol: ","
    });
  }
};
</script>
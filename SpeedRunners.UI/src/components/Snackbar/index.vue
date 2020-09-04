<template>
  <v-snackbar v-model="show" :color="color" :top="true" :timeout="timeout">
    {{ message }}
    <v-btn text @click="show = false">Close</v-btn>
  </v-snackbar>
</template>
<script>
export default {
  name: "Snackbar",
  data() {
    return {
      show: true,
      color: "success",
      message: "",
      timeout: 6000
    };
  }
};
</script>
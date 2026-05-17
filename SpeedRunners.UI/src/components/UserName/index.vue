<template>
  <component
    :is="tag"
    :class="['user-name', { clickable: !!platformID }]"
    @click.stop="onClick"
  >{{ personaName || platformID }}</component>
</template>

<script>
import { goToUserProfile } from "@/utils/profile";

export default {
  name: "UserName",
  props: {
    platformID: { type: String, default: "" },
    personaName: { type: String, default: "" },
    tag: { type: String, default: "span" }
  },
  methods: {
    onClick() {
      goToUserProfile(this.$router, this.platformID);
    }
  }
};
</script>

<style scoped>
.user-name.clickable {
  cursor: pointer;
}
.user-name.clickable:hover {
  text-decoration: underline;
}
</style>

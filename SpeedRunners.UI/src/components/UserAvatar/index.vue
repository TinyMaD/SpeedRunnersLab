<template>
  <v-avatar
    :size="size"
    :class="{ clickable: !!platformID }"
    @click.stop="onClick"
  >
    <v-img v-if="avatarUrl" :src="avatarUrl" />
    <v-icon v-else>mdi-account-circle</v-icon>
  </v-avatar>
</template>

<script>
import { goToUserProfile } from "@/utils/profile";

export default {
  name: "UserAvatar",
  props: {
    platformID: { type: String, default: "" },
    avatarUrl: { type: String, default: "" },
    size: { type: [Number, String], default: 36 }
  },
  methods: {
    onClick() {
      goToUserProfile(this.$router, this.platformID);
    }
  }
};
</script>

<style scoped>
.clickable {
  cursor: pointer;
  transition: opacity 0.15s;
}
.clickable:hover {
  opacity: 0.85;
}
</style>

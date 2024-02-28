<template>
  <v-container fluid>
    <v-row justify="center">
      <v-card width="940px" min-height="800px" style="padding:10px" dark>
        <VTimeline class="timeline" dense>
          <VSlideYTransition group>
            <VTimelineItem
              v-for="(eachPost, index) in timelinePost"
              :key="index"
              :color="getColor(eachPost.type)"
              :icon="getIcon(eachPost.type)"
              :small="isSmall(eachPost.type)"
              :large="isLarge(eachPost.type)"
            >
              <XCard>
                <VChip
                  style="position: absolute; top: -10px"
                  class="mr-2"
                  small
                  dark
                  :color="getColor(eachPost.type)"
                >
                  {{ eachPost.title }}</VChip>
                <div class="subheading mt-2">
                  <span class="px-1">
                    {{ eachPost.version }}
                  </span>
                  <span class="px-1">
                    {{ eachPost.date }}
                  </span>
                </div>
                <div class="caption mb-2">
                  <div
                    v-for="eachType in [
                      'stone',
                      'cloud',
                      'feature',
                      'fix',
                      'beautify',
                      'other'
                    ]"
                    :key="eachType"
                  >
                    <div v-if="postDetails(eachPost.list,eachType).length != 0">
                      <div class="body-2 font-weight-bold">
                        <VIcon left small>{{ getListIcon(eachType) }} </VIcon>
                        <span style="vertical-align: middle">{{
                          getTypeName(eachType)
                        }}</span>
                      </div>
                      <li
                        v-for="eachItem in postDetails(eachPost.list,eachType)"
                        :key="eachItem.text"
                        class="ml-2"
                        style="list-style-type:circle"
                      >
                        {{ eachItem.text }}
                      </li>
                    </div>
                  </div>
                </div>
                <v-img
                  :src="eachPost.img"
                /></XCard>
            </VTimelineItem>
          </VSlideYTransition>
        </VTimeline>
      </v-card>
    </v-row>
  </v-container>
</template>
<script>
import axios from "axios";
import XCard from "@/components/XCard";

export default {
  components: {
    XCard
  },
  data() {
    return {
      timelinePost: []
    };
  },
  mounted() {
    axios.get(`/log.json?timestamp=${new Date().getTime()}`)
      .then(response => {
        this.timelinePost = response.data;
      })
      .catch(error => {
        console.log(error);
      });
  },
  methods: {
    postDetails(list, type) {
      return list.filter(x => x.type === type);
    },
    getTypeName(name) {
      switch (name) {
        case "stone":
          return "里程碑";
        case "feature":
          return "特性";
        case "fix":
          return "修复";
        case "beautify":
          return "美化";
        case "cloud":
          return "架构";
        default:
          return "其他";
      }
    },
    isLarge: function(type) {
      let isLarge;
      type === "holyshit" ? (isLarge = true) : (isLarge = false);
      return isLarge;
    },
    isSmall: function(type) {
      let isSmall;
      type === "unimportant" ? (isSmall = true) : (isSmall = false);
      return isSmall;
    },
    getIcon: function(type) {
      switch (type) {
        case "holyshit":
          return "mdi-nuke";
        case "important":
          return "mdi-flag";
        case "ordinary":
          return "mdi-chess-rook";
        case "unimportant":
          return "";
        default:
          break;
      }
    },
    getListIcon: function(type) {
      switch (type) {
        case "feature":
          return "mdi-lightbulb-on";
        case "fix":
          return "mdi-bug";
        case "beautify":
          return "mdi-flower";
        case "stone":
          return "mdi-rocket";
        case "cloud":
          return "mdi-cloud";
        case "other":
          return "mdi-wrench";
        default:
          break;
      }
    },
    getColor: function(type) {
      switch (type) {
        case "ordinary":
          return "orange lighten-1";
        case "unimportant":
          return "green lighten-1";
        case "holyshit":
          return "red lighten-1";
        case "important":
          return "blue lighten-1";
        default:
          break;
      }
    }
  }
};
</script>
<style scoped>
.timeline {
  margin: 0 10px;
}
</style>
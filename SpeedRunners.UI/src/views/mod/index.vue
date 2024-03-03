<template>
  <v-container fluid>
    <v-row justify="center">
      <v-card width="1500px" min-height="800px" class="d-flex flex-row" dark>
        <div>
          <v-list width="130px">
            <v-list-item>
              <v-btn block color="primary" :small="isZh ? false : true" @click="clickUpload">
                <v-icon>mdi-upload</v-icon>{{ $t('mod.upload') }}
              </v-btn>
            </v-list-item>
            <v-divider />
            <!-- <v-list-group>
              <template v-slot:activator>
                <v-list-item-title>人 物</v-list-item-title>
              </template>
              <v-list-item
                v-for="(character, i) in characters"
                :key="i"
                link
              >
                <v-list-item-title class="text-center body-2" v-text="character" />
              </v-list-item>
            </v-list-group> -->
            <v-list-item-group v-model="searchParam.tag" mandatory>
              <v-list-item
                v-for="(menu, i) in otherModMenu"
                :key="i"
                @click="changeList"
              >
                <v-list-item-title>{{ menu }}</v-list-item-title>
              </v-list-item>
            </v-list-item-group>
          </v-list>
        </div>
        <div
          fluid
          class="pa-2"
          style="background-color:rgb(66,66,66);width:1370px"
        >
          <v-row no-gutters>
            <v-col fluid>
              <v-text-field
                v-model="searchParam.keywords"
                color="success"
                :label="$t('common.search')"
                :hint=" $t('common.keywords') "
                append-icon="mdi-magnify"
                clearable
                @click:append="changeList"
                @keyup.enter.native="changeList"
              />
            </v-col>
            <div style="width:180px;margin-left:20px">
              <v-switch
                v-model="switchValue"
                :label="$t('mod.yourStar')"
                color="orange"
                @change="changeSwitch"
              />
            </div>
          </v-row>
          <template>
            <v-hover v-for="mod in data.list" :key="mod.id" v-slot="{ hover }">
              <v-card
                :key="mod.id"
                :elevation="hover ? 12 : 2"
                width="250px"
                height="200px"
                class="ma-2 float-left"
              >
                <v-img
                  :src="mod.imgUrl"
                  class="mod-img white--text align-end"
                  gradient="to bottom, rgba(0,0,0,0), rgba(0,0,0,.5)"
                  height="160px"
                >
                  <v-card-title
                    class="text-caption"
                    style="color:rgba(255,255,255,0.8)"
                  >
                    {{ mod.title }}
                  </v-card-title>
                  <v-fade-transition>
                    <v-overlay v-if="hover" absolute color="black">
                      <v-btn
                        icon
                        class="download-btn"
                        @click.stop="download(mod.title, mod.fileUrl)"
                      >
                        <v-icon>mdi-download</v-icon>
                      </v-btn>
                    </v-overlay>
                  </v-fade-transition>
                </v-img>

                <v-card-actions>
                  <v-btn
                    text
                    x-small
                    @click.stop="download(mod.title, mod.fileUrl)"
                  >
                    <v-icon>mdi-download</v-icon>
                    {{ mod.download }}
                  </v-btn>
                  <v-btn
                    text
                    x-small
                    :color="mod.star ? 'orange' : 'white'"
                    @click.stop="doStar(mod)"
                  >
                    <v-icon>{{ starIcon(mod.star) }}</v-icon>
                    {{ mod.starCount }}
                  </v-btn>
                  <v-spacer />
                  <div class="text-caption">
                    {{ fileSize(mod.size) }}
                  </div>
                </v-card-actions>
              </v-card>
            </v-hover>
          </template>
          <div class="text-center">
            <v-pagination
              v-show="pageCount > 1"
              v-model="searchParam.pageNo"
              circle
              :length="pageCount"
              :total-visible="7"
              @input="getList"
            />
          </div>
        </div>
      </v-card>
    </v-row>
    <ModInfo :visible.sync="drawer" @success="getList" />
  </v-container>
</template>
<script>
import { mapGetters } from "vuex";
import ModInfo from "./modInfo";
import { getDownloadUrl, getModList, operateModStar } from "@/api/asset";

export default {
  name: "Mod",
  components: {
    ModInfo
  },
  data: () => ({
    characters: [
      "Speedrunner",
      "Unic",
      "Cosmonaut",
      "Comrade",
      "Hothead",
      "Moonraker",
      "Buckshot",
      "Gil",
      "Falcon",
      "Neko",
      "Scout",
      "SkullDuggery",
      "Salem"
    ],
    transparent: "rgba(255, 255, 255, 0)",
    switchValue: false,
    searchParam: {
      onlyStar: false,
      tag: 0,
      pageNo: 1,
      pageSize: 15,
      keywords: ""
    },
    data: {
      list: [],
      total: 0
    },
    drawer: false
  }),
  computed: {
    ...mapGetters(["name"]),
    isZh() {
      return this.$i18n.locale === "zh";
    },
    otherModMenu() {
      return [this.$t("mod.character"), this.$t("mod.trail"), this.$t("mod.item"), "HUD", this.$t("mod.sound"), this.$t("mod.bg"), this.$t("mod.other")];
    },
    stepTitle() {
      return this.progress > 0
        ? `正在上传，请耐心等待...${this.progress} %`
        : "上传MOD资源";
    },
    pageCount() {
      return this.data.total === 0
        ? 1
        : parseInt((this.data.total - 1) / this.searchParam.pageSize) + 1;
    },
    fileSize() {
      return function(fileLength) {
        var unit = ["B", "KB", "MB", "GB", "TB"];
        var unitIndex = 0;
        while (fileLength >= 1024) {
          fileLength /= 1024;
          unitIndex++;
        }
        return parseInt(fileLength) + " " + unit[unitIndex];
      };
    },
    starIcon() {
      return function(star) {
        return star ? "mdi-star" : "mdi-star-outline";
      };
    }
  },
  mounted() {
    this.$nextTick(function() {
      this.getList();
    });
  },
  methods: {
    changeList() {
      this.searchParam.pageNo = 1;
      this.getList();
    },
    getList() {
      getModList(this.searchParam).then(res => {
        this.data = res.data;
      });
    },
    changeSwitch() {
      if (this.name === "") {
        this.$toast.info("请先登录再操作");
        this.switchValue = false;
        return;
      }
      this.searchParam.onlyStar = this.switchValue;
      this.searchParam.pageNo = 1;
      this.getList();
    },
    clickUpload() {
      if (this.name !== "") {
        this.drawer = true;
      } else {
        this.$toast.info("请登录后再上传");
      }
    },
    doStar(mod) {
      if (this.name === "") {
        this.$toast.info("请登录后再收藏");
        return;
      }
      operateModStar(mod.id, !mod.star).then(res => {
        this.$toast.success(`${mod.star ? "取消" : ""}收藏成功`);
        this.getList();
      });
    },
    download(title, fileUrl) {
      title = `${title}.${fileUrl.split(".").pop()}`;
      getDownloadUrl(fileUrl).then(response => {
        var x = new XMLHttpRequest();
        x.open("GET", response.data, true);
        x.responseType = "blob";
        x.onload = function(e) {
          var url = window.URL.createObjectURL(x.response);
          var a = document.createElement("a");
          a.href = url;
          a.download = title;
          a.click();
        };
        x.send();
      });
    }
  },
  metaInfo() {
    return {
      meta: [
        {
          vmid: "keywords",
          name: "keywords",
          content: "游戏MOD,轨迹,Trail,Mod of SpeedRunners,HUD," + this.$baseKeywords
        }
      ]
    };
  }
};
</script>
<style scoped>
.mod-img {
  transition: opacity 0.4s ease-in-out;
}
.download-btn {
  width: 100px;
  height: 100px;
}
.download-btn .v-icon {
  font-size: 50px;
}
</style>
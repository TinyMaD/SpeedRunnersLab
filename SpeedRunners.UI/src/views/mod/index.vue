<template>
  <v-container fluid>
    <v-row justify="center">
      <v-card width="1500px" min-height="800px" class="d-flex flex-row">
        <div>
          <v-list width="130px">
            <v-list-item>
              <v-btn block color="primary" :small="isZh ? false : true" @click="clickUpload">
                <v-icon>mdi-upload</v-icon>{{ $t('mod.upload') }}
              </v-btn>
            </v-list-item>
            <v-divider />
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
          :style="{backgroundColor:$vuetify.theme.dark?'rgb(66,66,66)':'rgb(240,240,240)',width:'1370px'}"
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
                  :gradient="$vuetify.theme.dark?'to bottom, rgba(0,0,0,0), rgba(0,0,0,.5)':'to bottom, rgba(255,255,255,0), rgba(255,255,255,.5)'"
                  height="160px"
                >
                  <v-card-title
                    v-if="mod.isNew"
                    class="pb-6 mb-16 pl-2"
                  >
                    <v-icon color="indigo darken-3" size="36px" style="background-color: white;height: 21px;width: 27px;">mdi-new-box</v-icon>
                  </v-card-title>
                  <v-card-title
                    class="text-caption"
                    :style="{color:$vuetify.theme.dark?'rgba(255,255,255,0.8)':'rgba(0,0,0,0.8)'}"
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
                  <v-tooltip bottom>
                    <template v-slot:activator="{ on, attrs }">
                      <v-btn
                        text
                        x-small
                        v-bind="attrs"
                        @click.stop="download(mod.title, mod.fileUrl)"
                        v-on="on"
                      ><v-icon>mdi-download</v-icon>
                        {{ mod.download }}
                      </v-btn>
                    </template>
                    <span>{{ $t('mod.download') }}</span>
                  </v-tooltip>

                  <v-tooltip bottom>
                    <template v-slot:activator="{ on, attrs }">
                      <v-btn
                        v-bind="attrs"
                        text
                        x-small
                        :color="mod.star ? 'orange' : $vuetify.theme.dark?'white':'#333'"
                        v-on="on"
                        @click.stop="doStar(mod)"
                      >
                        <v-icon>{{ starIcon(mod.star) }}</v-icon>
                        {{ mod.starCount }}
                      </v-btn>
                    </template>
                    <span>{{ starTooltip(mod.star) }}</span>
                  </v-tooltip>

                  <v-tooltip v-if="mod.authorID === steamId" bottom>
                    <template v-slot:activator="{ on, attrs }">
                      <v-hover v-slot="{ hover }">
                        <v-btn
                          text
                          x-small
                          :color="hover?'red':$vuetify.theme.dark?'white':'#333'"
                          v-bind="attrs"
                          @click.stop="openDialog(mod.id)"
                          v-on="on"
                        ><v-icon>mdi-delete-outline</v-icon>
                        </v-btn>
                      </v-hover>
                    </template>
                    <span>{{ $t('common.delete') }}</span>
                  </v-tooltip>

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
    <v-dialog
      v-model="dialog"
      max-width="290"
    >
      <v-card>
        <v-card-title class="text-h5">
          {{ $t('mod.deleteWarn') }}
        </v-card-title>
        <v-card-text>{{ $t('mod.deleteInfo') }}</v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-hover v-slot="{ hover }">
            <v-btn
              :loading="deleteLoading"
              color="red"
              :text="!hover"
              @click="deleteMod"
            >
              {{ $t('common.confirm') }}
            </v-btn>
          </v-hover>
          <v-btn
            text
            @click="dialog = false"
          >
            {{ $t('common.cancel') }}
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
    <ModInfo :visible.sync="drawer" @success="getList" />
  </v-container>
</template>
<script>
import { mapGetters } from "vuex";
import ModInfo from "./modInfo";
import { getDownloadUrl, getModList, operateModStar, deleteMod } from "@/api/asset";

export default {
  name: "Mod",
  components: {
    ModInfo
  },
  data: () => ({
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
    drawer: false,
    dialog: false,
    deleteLoading: false,
    deleteID: 0
  }),
  computed: {
    ...mapGetters(["name", "steamId"]),
    isZh() {
      return this.$i18n.locale === "zh";
    },
    otherModMenu() {
      return [this.$t("mod.character"), this.$t("mod.trail"), this.$t("mod.item"), "HUD", this.$t("mod.sound"), this.$t("mod.bg"), this.$t("mod.other")];
    },
    stepTitle() {
      return this.progress > 0
        ? this.$t("mod.uploadingTitle", [this.progress])
        : this.$t("mod.loadTitle");
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
    starTooltip(star) {
      return star ? this.$t("mod.star") : this.$t("mod.unStar");
    },
    changeList() {
      this.searchParam.pageNo = 1;
      this.getList();
    },
    getList() {
      getModList(this.searchParam).then(res => {
        this.data = res.data;
      });
    },
    openDialog(modId) {
      this.deleteID = modId;
      this.dialog = true;
    },
    deleteMod() {
      this.deleteLoading = true;
      var modID = this.deleteID;
      deleteMod({ modID }).then(res => {
        this.deleteLoading = false;
        this.dialog = false;
        if (res.code === 666) {
          this.$toast.success(this.$t("mod.deleteSucc"));
          this.getList();
        }
      });
    },
    changeSwitch() {
      if (this.name === "") {
        this.$toast.info(this.$t("common.loginPlz"));
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
        this.$toast.info(this.$t("common.loginPlz"));
      }
    },
    doStar(mod) {
      if (this.name === "") {
        this.$toast.info(this.$t("common.loginPlz"));
        return;
      }
      operateModStar(mod.id, !mod.star).then(res => {
        this.$toast.success(mod.star ? this.$t("mod.unstarSucc") : this.$t("mod.starSucc"));
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
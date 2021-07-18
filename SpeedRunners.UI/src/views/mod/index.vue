<template>
  <v-container fluid>
    <v-row>
      <v-col cols="12">
        <v-row justify="center">
          <v-card width="1500px" min-height="800px" class="d-flex flex-row" dark>
            <div>
              <v-list width="130px">
                <v-list-group>
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
                </v-list-group>
                <v-list-item
                  v-for="(menu, i) in otherModMenu"
                  :key="i"
                  link
                >
                  <v-list-item-title v-text="menu" />
                </v-list-item>
              </v-list>
            </div>
            <div fluid class="d-flex flex-row pa-2" style="border-left:1px solid rgb(205,205,205)">
              <v-card
                v-for="mod in 12"
                :key="mod.id"
                width="250px"
                height="210px"
                class="ma-2"
                color="secondary"
              >
                <v-img
                  src="https://cdn.pixabay.com/photo/2020/07/12/07/47/bee-5396362_1280.jpg"
                  class="white--text align-end"
                  gradient="to bottom, rgba(0,0,0,.1), rgba(0,0,0,.5)"
                  height="160px"
                >
                  <v-card-title v-text="mod.id" />
                </v-img>

                <v-card-actions>
                  <v-spacer />

                  <v-btn icon>
                    <v-icon>mdi-heart</v-icon>
                  </v-btn>

                  <v-btn icon>
                    <v-icon>mdi-bookmark</v-icon>
                  </v-btn>

                  <v-btn icon>
                    <v-icon>mdi-share-variant</v-icon>
                  </v-btn>
                </v-card-actions>
              </v-card>
            </div>
          </v-card>
        </v-row>
      </v-col>
    </v-row>
    <div class="text-center">
      <v-btn
        color="primary"
        dark
        @click="download"
      >
        下 载
      </v-btn>
      <v-dialog
        v-model="dialog"
        persistent
        dark
        width="500"
      >
        <template v-slot:activator="{ on, attrs }">
          <v-btn
            color="primary"
            dark
            v-bind="attrs"
            v-on="on"
          >
            上 传
          </v-btn>
        </template>

        <v-card>
          <v-card-title>
            {{ stepTitle }}
          </v-card-title>
          <v-progress-linear
            v-model="progress"
          />
          <v-file-input
            :show-size="true"
            accept="image/*"
            label="点击选择文件"
            @change="changeFiles"
          />
          <v-divider />
          <v-card-actions>
            <v-spacer />
            <v-btn
              color="primary"
              text
              @click="dialog = false"
            >
              取 消
            </v-btn>
            <v-btn
              color="primary"
              @click="uploadFile"
            >
              提 交
            </v-btn>
          </v-card-actions>
        </v-card>
      </v-dialog>
    </div>
  </v-container>
</template>
<script>
import * as qiniu from "qiniu-js";
import { getUploadToken, getDownloadUrl, getModList } from "@/api/asset";

export default {
  name: "Mod",
  data: () => ({
    characters: ["Speedrunner", "Unic", "Cosmonaut", "Comrade", "Hothead", "Moonraker", "Buckshot", "Gil", "Falcon", "Neko", "Scout", "SkullDuggery", "Salem"],
    otherModMenu: ["轨 迹", "道 具", "HUD", "音 效", "背 景", "其 它"],
    dialog: false,
    file: {},
    subscription: null,
    progress: 0,
    searchParam: {
      tag: 0,
      pageNo: 1,
      pageSize: 10,
      keywords: ""
    },
    list: []
  }),
  computed: {
    stepTitle() {
      return this.progress > 0 ? `正在上传，请耐心等待...${this.progress} %` : "上传MOD资源";
    }
  },
  mounted() {
    this.$nextTick(function() {
      this.getList();
    });
  },
  methods: {
    getList() {
      getModList(this.searchParam).then(res => {
        console.log(res.data);
        this.list = res.data;
      });
    },
    changeFiles(file) {
      this.file = file;
    },
    uploadFile() {
      var that = this;
      getUploadToken().then(response => {
        const token = response.data;
        // const putExtra = {
        //    fname: "qiniu.txt"
        //    customVars: { "x:test": "qiniu" },
        //    metadata: { "x-qn-meta": "qiniu" }
        // };
        const config = {
          useCdnDomain: true
        };
        const observable = qiniu.upload(that.file, that.file.name, token, null, config);
        const observer = {
          next(res) {
            that.progress = res.total.percent;
          },
          error(err) {
            that.$toast.error(`${err.name}:${err.message}`);
          },
          complete(res) {
            that.progress = 100;
            console.log(res);
          }
        };
        that.subscription = observable.subscribe(observer);
      });
    },

    download(name, downloadPath) {
      name = "TIM图片20200627145348.jpg";
      getDownloadUrl(name).then(response => {
        var x = new XMLHttpRequest();
        x.open("GET", response.data, true);
        x.responseType = "blob";
        x.onload = function(e) {
          var url = window.URL.createObjectURL(x.response);
          var a = document.createElement("a");
          a.href = url;
          a.download = name;
          a.click();
        };
        x.send();
      });
    }
  }
};
</script>
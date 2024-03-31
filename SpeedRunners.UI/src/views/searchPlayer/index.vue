<template>
  <v-container fluid>
    <v-row justify="center">
      <v-card width="940px" min-height="800px" style="padding:10px">
        <v-text-field
          v-model="keyWords"
          :loading="loading"
          color="success"
          :label="$t('common.search')"
          :hint="$t('stats.enterSteamID')"
          append-icon="mdi-magnify"
          clearable
          @click:append="searchPlayer"
          @keyup.enter.native="searchPlayer"
        />
        <div
          v-if="searchResult==null||searchResult.isGameInfo==undefined||(searchResult.isGameInfo===false&&searchResult.playerList.length===0)"
          class="transition-swing"
          v-text="content"
        />
        <v-simple-table v-if="searchResult!=null&&searchResult.isGameInfo">
          <template v-slot:default>
            <tbody>
              <tr v-for="item in searchResult.gameInfo.stats" :key="item.name">
                <td>{{ item.name }}</td>
                <td>{{ item.value }}</td>
              </tr>
            </tbody>
          </template>
        </v-simple-table>
        <v-list
          v-if="searchResult!=null&&searchResult.isGameInfo===false&&searchResult.playerList.length>0"
          subheader
        >
          <v-subheader>{{ $t('mod.invalidWarn') }}</v-subheader>
          <v-list-item
            v-for="(item, index) in searchResult.playerList"
            :key="index"
            @click="searchByID(item)"
          >
            <v-list-item-avatar>
              <v-img :src="item.avatar" />
            </v-list-item-avatar>
            <v-list-item-content>
              <v-list-item-title>{{ item.userName }}</v-list-item-title>
            </v-list-item-content>
          </v-list-item>
          <v-list-item
            v-if="searchResult.total>0&&!loadList&&searchResult.total>searchResult.pageNo*20"
            @click="getPlayerList()"
          >
            <v-list-item-content>
              <v-list-item-title>{{ loadingText }}</v-list-item-title>
            </v-list-item-content>
          </v-list-item>
        </v-list>
        <div v-if="loadList">
          <v-skeleton-loader
            v-for="item in 5"
            :key="item"
            type="list-item-avatar"
            class="mx-auto"
          />
        </div>
      </v-card>
    </v-row>
  </v-container>
</template>
<script>
import {
  searchPlayer,
  getPlayerList,
  searchPlayerByUrl,
  searchPlayerBySteamID64
} from "@/api/steam";
export default {
  name: "SearchPlayer",
  data() {
    return {
      keyWords: "",
      loading: false,
      loadList: false,
      searchResult: {
        gameInfo: {},
        sessionID: "",
        total: 0,
        pageNo: 1,
        playerList: []
      },
      contentType: 0
    };
  },
  computed: {
    content() {
      return this.contentType
        ? this.$t("stats.noResult")
        : this.$t("stats.content");
    },
    loadingText() {
      return this.$t("stats.showMore", [this.searchResult.total - this.searchResult.pageNo * 20]);
    }
  },
  methods: {
    searchPlayer() {
      if (this.keyWords.trim() === "") {
        return;
      }
      this.loading = true;
      searchPlayer(this.keyWords).then(response => {
        var result = response.data;
        this.searchResult = result;
        this.loading = false;
      });
    },
    searchByID(player) {
      this.loading = true;
      if (player.profilesOrID === "profiles") {
        searchPlayerBySteamID64(player.contentOfID).then(response => {
          var result = response.data;
          if (
            result === null ||
            (result.isGameInfo === false && result.playerList.length === 0)
          ) {
            this.contentType = 1;
          }
          this.searchResult = result;
          this.loading = false;
        });
      } else {
        searchPlayerByUrl(player.contentOfID).then(response => {
          var result = response.data;
          if (
            result === null ||
            (result.isGameInfo === false && result.playerList.length === 0)
          ) {
            this.contentType = 1;
          }
          this.searchResult = result;
          this.loading = false;
        });
      }
    },
    getPlayerList() {
      this.loadList = true;
      var param = this.searchResult;
      getPlayerList(this.keyWords, param.sessionID, param.pageNo + 1).then(
        response => {
          response.data.playerList = this.searchResult.playerList.concat(
            response.data.playerList
          );
          this.searchResult = response.data;
          this.loadList = false;
        }
      );
    }
  },
  metaInfo() {
    return {
      meta: [
        {
          vmid: "keywords",
          name: "keywords",
          content: "SR游戏数据查询,SpeedRunners玩家数据查询," + this.$baseKeywords
        }
      ]
    };
  }
};
</script>
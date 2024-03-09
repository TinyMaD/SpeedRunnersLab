<template>
  <v-container>
    <v-row justify="center">
      <table class="table">
        <tr id="tableHead" class="even">
          <th id="thf" width="10%" />
          <th id="online" width="13%" style="text-align:center;color:#81A636" />
          <th width="33%">{{ $t('rank.personaName') }}</th>
          <th width="25%">{{ $t('rank.tier') }}</th>
          <th v-if="pc < 1640" id="thl" width="19%">{{ $t('rank.score') }}</th>
          <th v-else width="13%">{{ $t('rank.score') }}</th>
          <th v-if="pc > 1640" id="thl" width="6%" />
        </tr>
        <tbody id="tbody">
          <tr v-for="(player, index) in response.data" :key="index" :class="index % 2 === 0 ? 'odd' : 'even'">
            <td v-if="index < 3"><img class="rowImg" :src="`img/${index + 1}.png`"></td>
            <td v-else>{{ index }} </td>
            <td class="avator">
              <div class="avatorImg" :style="getAvatorStyle(player)">
                <div v-if="isPlayingSR(player)" class="right">
                  <img class="srIndex" style="height:20px;margin:27px -15px 0 0" src="img/srstate.png">
                </div>
              </div>
            </td>
            <td style="white-space: nowrap;text-overflow: ellipsis;overflow: hidden;">{{ player.personaName }}</td>
            <td>
              <div class="levelAll">
                <div class="levelDiv left">
                  <img class="level" src="img/LeagueBadgesMedium.png" :style="getLevelStyle(player.rankLevel)">
                </div>
                <div class="right levelName">{{ getLevelName(player.rankLevel) }}</div>
              </div>
            </td>
            <td>{{ player.rankScore }}{{ scoreIncrease(player) }}</td>
            <td v-if="pc > 1640">{{ scoreIncrease(player) }}</td>
          </tr>
        </tbody>
        <tr id="tfoot" class="even">
          <td v-if="pc < 1640" id="tfootf" />
          <td v-if="pc < 1640" />
          <td v-if="pc < 1640" id="tfootl" colspan="3">{{ $t('rank.updateInfo') }}</td>
          <td v-if="pc > 1640" id="tfootf" />
          <td v-if="pc > 1640" />
          <td v-if="pc > 1640" />
          <td v-if="pc > 1640" id="tfootl" colspan="3">{{ $t('rank.updateInfo') }}</td>
        </tr>
      </table>
    </v-row>
  </v-container>
</template>

<script>
import { getRankList } from "@/api/rank";
import $ from "jquery";
export default {
  name: "Rank",

  data() {
    return {
      response: {},
      pc: 0,
      imgSize: 150,
      levelName: "",
      t: 0,
      right: 0,
      bottom: 0,
      left: 0,
      srstate: "",
      personstate: "",
      moblieUpScore: "",
      pcUpScore: "",
      leveMarginTop: 6
    };
  },

  computed: {
    scoreIncrease() {
      return player => {
        const increaseScore = player.rankScore - player.oldRankScore;
        if (increaseScore > 0) {
          return `↑${increaseScore}`;
        }
        return "";
      };
    }
  },

  mounted() {
    this.pc = $(window).width();
    if (this.pc < 451) {
      this.imgSize = 120;
      this.leveMarginTop = 11;
    }
    getRankList().then(res => {
      this.response = res;
    });
  },

  methods: {
    getAvatorStyle(player) {
      let borderColor = "#4C94B0";
      if (player.state === 0) {
        borderColor = "#616161";
      } else if (player.gameID !== "" && player.gameID != null) {
        borderColor = "#81A636";
      }
      const style = `width: 41px; height: 41px; border: 2px solid ${borderColor}; border-radius: 2px; background-image: url(${player.avatarM}); background-size: 37px; margin: auto;`;
      return style;
    },
    isPlayingSR(player) {
      return player.gameID === "207140";
    },
    getLevelStyle(rankLevel) {
      const height = (4 * this.imgSize) / 3;
      const width = this.imgSize;
      let t, right, bottom, left;
      switch (rankLevel) {
        case 0:
          t = 0;
          right = width / 3;
          bottom = height / 4;
          left = 0;
          break;
        case 1:
          t = 0;
          right = (width * 2) / 3;
          bottom = height / 4;
          left = width / 3;
          break;
        case 2:
          t = 0;
          right = width;
          bottom = height / 4;
          left = (width * 2) / 3;
          break;
        case 3:
          t = height / 4;
          right = width / 3;
          bottom = height / 2;
          left = 0;
          break;
        case 4:
          t = height / 4;
          right = (width * 2) / 3;
          bottom = height / 2;
          left = width / 3;
          break;
        case 5:
          t = height / 4;
          right = width;
          bottom = height / 2;
          left = (width * 2) / 3;
          break;
        case 6:
          t = height / 2;
          right = width / 3;
          bottom = (height * 3) / 4;
          left = 0;
          break;
        case 7:
          t = height / 2;
          right = (width * 2) / 3;
          bottom = (height * 3) / 4;
          left = width / 3;
          break;
        case 8:
          t = height * 0.5;
          right = width;
          bottom = (height * 3) / 4;
          left = (width * 2) / 3;
          break;
        case 9:
          t = (height * 3) / 4;
          right = width / 3;
          bottom = height;
          left = 0;
          break;
        default:
          break;
      }
      return `clip: rect(${t}px ${right}px ${bottom}px ${left}px); margin: ${-1 * t + this.leveMarginTop}px 0 0 ${-1 * left}px; width: ${this.imgSize}px;`;
    },
    getLevelName(rankLevel) {
      switch (rankLevel) {
        case 0:
          return this.$t("rank.entry");
        case 1:
          return this.$t("rank.beginner");
        case 2:
          return this.$t("rank.advanced");
        case 3:
          return this.$t("rank.expert");
        case 4:
          return this.$t("rank.bronze");
        case 5:
          return this.$t("rank.silver");
        case 6:
          return this.$t("rank.gold");
        case 7:
          return this.$t("rank.platinum");
        case 8:
          return this.$t("rank.diamond");
        case 9:
          return "KOS";
        default:
          return "";
      }
    }
  }
};
</script>

<style>
#addedDiv {
  width: 25%;
}

.rowImg {
  height: 40px;
  width: 40px;
}

#head {
  width: 100%;
  height: 105px;
  background-color: rgb(23, 26, 33);
}

#title {
  background-color: rgb(23, 26, 33);
  width: 100%;
  max-width: 940px;
  height: 105px;
  margin: 0 auto;
}

#bg {
  width: 100%;
  margin-top: 300px;
}

#srname {
  height: 150px;
  margin-top: -30px;
  margin-left: -20px;
  margin-bottom: -50px;
}

#titlec {
  height: 40px;
}

#ttdiv {
  margin: 60px 0 0 -30px;
}

.left {
  float: left;
}

.right {
  float: right;
}

#loginL {
  height: 66px;
  width: 109px;
  cursor: pointer;
}

#tishi {
  color: rgb(189, 187, 185);
  font-size: 10px;
  margin: 10px 0 5px 0;
}

#foot {
  color: rgb(189, 187, 185);
  font-size: 10px;
}

.foot {
  width: 100%;
  text-align: center;
}

#logdiv {
  height: 105px;
  width: 109px;
}

#floatDiv {
  width: 50%;
}

#rankDiv {
  margin-top: 300px;
  width: 99.5%;
  max-width: 940px;
  margin: 0 auto;
  border: 1px solid rgb(52, 52, 52);
  border-radius: 10px;
}

.table {
  color: rgb(198, 212, 223);
  border-collapse: collapse;
  border-spacing: 0;
  margin: 0;
  padding: 0;
  width: 100%;
  max-width: 940px;
  text-align: center;
  vertical-align: middle;
  table-layout: fixed;
}

.tableS {
  color: rgb(198, 212, 223);
  border-collapse: collapse;
  border-spacing: 0;
  margin: 0;
  padding: 0;
  width: 100%;
  text-align: center;
  vertical-align: middle;
  font-size: 30%;
}

td {
  align-content: center;
}

th {
  height: 40px;
}

#thf {
  border-radius: 10px 0 0 0;
}

#thl {
  border-radius: 0 10px 0 0;
}

.avator {
  height: 56px;
  padding-top: 4px;
}

.avator img {
  height: 50px;
}

.level {
  position: absolute;
}

.levelAll {
  height: 60px;
  width: 120px;
  margin: 0 auto;
}

.levelDiv {
  width: 60px;
  height: 60px;
  position: relative;
  text-align: left;
}

.levelName {
  width: 60px;
  height: 41px;
  padding-top: 19px;
}

#tfoot {
  text-align: right;
  height: 30px;
  color: rgb(189, 187, 185);
  font-size: 10px;
}

#tfootf {
  border-radius: 0 0 0 10px;
}

#tfootl {
  border-radius: 0 0 10px 0;
  padding-right: 10px;
}
.even {
  background-color: #262626;
}

.odd {
  background-color: #3E3E3E;
}
</style>
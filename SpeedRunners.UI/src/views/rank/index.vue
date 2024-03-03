<!--祖传代码，待优化-->
<template>
  <v-container>
    <v-row justify="center">
      <table class="table">
        <tr id="tableHead">
          <th id="thf" width="10%" />
          <th id="online" width="13%" style="text-align:center;color:#81A636" />
          <th width="33%">{{ $t('rank.personaName') }}</th>
          <th width="25%">{{ $t('rank.tier') }}</th>
          <th v-if="pc < 1640" id="thl" width="19%">{{ $t('rank.score') }}</th>
          <th v-else width="13%">{{ $t('rank.score') }}</th>
          <th v-if="pc > 1640" id="thl" width="6%" />
        </tr>
        <tbody id="tbody" />
        <tr id="tfoot">
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

  data: () => ({
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
  }),
  computed: {
    getlvlName(level) {
      switch (level) {
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
  },
  mounted() {
    this.pc = $(window).width();
    if (this.pc < 451) {
      this.imgSize = 120;
      this.leveMarginTop = 11;
    }
    getRankList().then(response => {
      this.response = response;
      this.showRank(response, this.pc);
    });
  },
  methods: {
    showRank(response, pc) {
      var res = response;
      // 输出列表
      var tbody = "";
      for (var i = 0; i < res.data.length; i++) {
        // 计算段位
        this.getImgIndex(res.data[i].rankLevel, this.imgSize);
        // 计算头像边框颜色
        this.getBorderColor(res.data[i].state, res.data[i].gameID);
        // 计算增长分数
        var increase = "";
        var increaseScore = res.data[i].rankScore - res.data[i].oldRankScore;
        if (increaseScore > 0) {
          increase = "↑" + increaseScore;
        }
        // 增长分显示位置
        if (pc < 1640) {
          this.moblieUpScore =
            "<div style='color:#00FF00'>" + increase + "</div>";
        } else {
          this.pcUpScore =
            "<td style='text-align:left;color:#00FF00'>" + increase + "</td>";
        }
        var row = i + 1;
        if (row < 4) {
          row = "<img class='rowImg' src='img/" + row + ".png' />";
        }
        var str = "<tr><td>" + row + "</td>";
        str +=
          "<td class='avator'><div class='avatorImg' style='width:41px;height:41px;border:2px solid " +
          this.personstate +
          ";border-radius:2px;background-image: url(" +
          res.data[i].avatarM +
          ");background-size: 37px;margin:auto;'><div class='right' style='display:" +
          this.srstate +
          ";'><img class='srIndex' style='height:20px;margin:27px -15px 0 0' src='img/srstate.png'/></div></div></td>";

        str +=
          "<td style='white-space: nowrap;text-overflow: ellipsis;overflow: hidden;'>" +
          res.data[i].personaName +
          "</td>";
        str +=
          "<td><div class='levelAll'><div class='levelDiv left'><img class='level' src='img/LeagueBadgesMedium.png' style='clip: rect(" +
          this.t +
          "px " +
          this.right +
          "px " +
          this.bottom +
          "px " +
          this.left +
          "px); margin:" +
          (-1 * this.t + this.leveMarginTop) +
          "px 0 0 " +
          -1 * this.left +
          "px;width:" +
          this.imgSize +
          "px' /></div><div class='right levelName'>" +
          this.levelName +
          "</div></div></td>";
        str +=
          "<td>" +
          res.data[i].rankScore +
          this.moblieUpScore +
          "</td>" +
          this.pcUpScore +
          "</tr>";
        tbody += str;
      }
      $("#tbody").html(tbody);
      if (this.pc < 415) {
        $(".levelDiv").width("53%");
        $(".levelName")
          .width("47%")
          .css("text-align", "left");
        $(".levelAll").width("100%");
        $(".avatorImg")
          .width("33px")
          .height("33px")
          .css("background-size", "33px");
        $(".srIndex")
          .height("17px")
          .css("margin", "22px -12px 0 0");
        $(".rowImg")
          .width("30px")
          .height("30px");
      }
      $(".table tr:even").css("background-color", "#262626");
      $(".table tr:odd").css("background-color", "#3E3E3E");
    },
    getBorderColor(State, GameID) {
      this.srstate = "none";
      switch (State) {
        case 0:
          // 离线，灰色
          this.personstate = "#616161";
          break;
        default:
          // 在线就查看是否在玩游戏
          if (GameID !== "" && GameID != null) {
            // 在玩游戏，绿色
            this.personstate = "#81A636";
            if (GameID === "207140") {
              // 在玩SR，显示sr角标
              this.srstate = "block";
            }
          } else {
            // 没玩游戏，蓝色
            this.personstate = "#4C94B0";
          }
          break;
      }
    },

    // 根据段位计算切片数据
    getImgIndex(level, width) {
      var height = (4 * width) / 3; // 图片长度
      switch (level) {
        case 0:
          this.levelName = this.$t("rank.entry");
          this.t = 0;
          this.right = width / 3;
          this.bottom = height / 4;
          this.left = 0;
          break;
        case 1:
          this.levelName = this.$t("rank.beginner");
          this.t = 0;
          this.right = (width * 2) / 3;
          this.bottom = height / 4;
          this.left = width / 3;
          break;
        case 2:
          this.levelName = this.$t("rank.advanced");
          this.t = 0;
          this.right = width;
          this.bottom = height / 4;
          this.left = (width * 2) / 3;
          break;
        case 3:
          this.levelName = this.$t("rank.expert");
          this.t = height / 4;
          this.right = width / 3;
          this.bottom = height / 2;
          this.left = 0;
          break;
        case 4:
          this.levelName = this.$t("rank.bronze");
          this.t = height / 4;
          this.right = (width * 2) / 3;
          this.bottom = height / 2;
          this.left = width / 3;
          break;
        case 5:
          this.levelName = this.$t("rank.silver");
          this.t = height / 4;
          this.right = width;
          this.bottom = height / 2;
          this.left = (width * 2) / 3;
          break;
        case 6:
          this.levelName = this.$t("rank.gold");
          this.t = height / 2;
          this.right = width / 3;
          this.bottom = (height * 3) / 4;
          this.left = 0;
          break;
        case 7:
          this.levelName = this.$t("rank.platinum");
          this.t = height / 2;
          this.right = (width * 2) / 3;
          this.bottom = (height * 3) / 4;
          this.left = width / 3;
          break;
        case 8:
          this.levelName = this.$t("rank.diamond");
          this.t = height * 0.5;
          this.right = width;
          this.bottom = (height * 3) / 4;
          this.left = (width * 2) / 3;
          break;
        case 9:
          this.levelName = "KOS";
          this.t = (height * 3) / 4;
          this.right = width / 3;
          this.bottom = height;
          this.left = 0;
          break;
        default:
      }
    }
  },
  metaInfo() {
    return {
      meta: [
        {
          vmid: "keywords",
          name: "keywords",
          content: "SpeedRunners排行榜,SpeedRunners天梯,Rank,league," + this.$baseKeywords
        }
      ]
    };
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
</style>
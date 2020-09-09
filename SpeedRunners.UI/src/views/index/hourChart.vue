<template>
  <div :class="className" :style="{height:totalHeight,width:width}" />
</template>

<script>
import resize from "@/utils/resize";
// 引入 ECharts 主模块
var echarts = require("echarts/lib/echarts");
// 引入柱状图
require("echarts/lib/chart/bar");
// 引入提示框和标题组件
require("echarts/lib/component/tooltip");
require("echarts/lib/component/title");
require("echarts/lib/component/dataset");
require("echarts/lib/component/grid");

import { getHourChart } from "@/api/rank";

// const animationDuration = 6000;

export default {
  mixins: [resize],
  props: {
    className: {
      type: String,
      default: "chart"
    },
    width: {
      type: String,
      default: "100%"
    },
    height: {
      type: Number,
      default: 35
    }
  },
  data() {
    return {
      chart: null,
      dataCount: 0,
      bottom: 20,
      list: [],
      richStyle: {}
    };
  },
  computed: {
    totalHeight() {
      return `${this.dataCount * this.height + this.bottom + 60}px`;
    }
  },
  mounted() {
    var that = this;
    getHourChart().then(response => {
      this.dataCount = response.data.length;
      this.list = response.data.reverse().map(function(x, i) {
        x.weekPlayTime = (x.weekPlayTime / 60).toFixed(1);
        if (x.weekPlayTime !== 0) {
          that.richStyle[`a${i}`] = {
            height: 25,
            align: "center",
            backgroundColor: { image: x.avatarS }
          };
          return x;
        }
      });
      this.$nextTick(() => {
        this.initChart();
      });
    });
  },
  beforeDestroy() {
    if (!this.chart) {
      return;
    }
    this.chart.dispose();
    this.chart = null;
  },
  methods: {
    initChart() {
      var that = this;
      this.chart = echarts.init(this.$el);
      this.chart.setOption({
        title: {
          text: "两周游戏时间",
          left: "center",
          top: 15,
          textStyle: {
            color: "#fff",
            fontWeight: 400
          }
        },
        grid: {
          bottom: that.bottom
        },
        tooltip: {
          formatter: function(a) {
            return a.data.personaName + "：" + a.data.weekPlayTime + " 小时";
          }
        },
        dataset: {
          source: that.list
        },
        xAxis: {},
        yAxis: {
          type: "category",
          axisLabel: {
            formatter: function(value, index) {
              return `{a${index}|}`;
            },
            rich: that.richStyle
          }
        },
        series: [
          {
            type: "bar",
            encode: {
              x: "weekPlayTime",
              y: "personaName"
            },
            itemStyle: {
              normal: {
                color: function(params) {
                  const colorList = ["#9b8bba", "#e098c7", "#8fd3e8", "#71669e", "#cc70af", "#7cb4cc"];
                  return colorList[(that.dataCount - params.dataIndex - 1) % 6];
                }
              }
            },
            label: {
              normal: {
                show: true,
                position: "right"
              }
            }
          }
        ]
      });
    }
  }
};
</script>
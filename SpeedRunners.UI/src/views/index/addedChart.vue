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

import { getAddedChart } from "@/api/rank";

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
    getAddedChart().then(response => {
      this.dataCount = response.data.length;
      this.list = response.data.reverse();
      this.list.forEach((x, i) => {
        that.richStyle[`a${i}`] = {
          height: 25,
          align: "center",
          backgroundColor: { image: x.avatarS }
        };
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
          text: "七日新增天梯分",
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
        tooltip: {},
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
              x: "rankScore",
              y: "personaName"
            },
            barWidth: 20,
            itemStyle: {
              color: function(params) {
                const colorList = ["#3fb1e3", "#6be6c1", "#626c91", "#a0a7e6", "#c4ebad", "#96dee8"];
                return colorList[(that.dataCount - params.dataIndex - 1) % 6];
              },
              barBorderRadius: [0, 10, 10, 0]
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
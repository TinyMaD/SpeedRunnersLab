-- ----------------------------
-- Table structure for Comment
-- ----------------------------
DROP TABLE IF EXISTS `Comment`;
CREATE TABLE `Comment`  (
  `ID` int(11) NOT NULL AUTO_INCREMENT,
  `PagePath` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '页面路径',
  `PlatformID` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '评论者平台ID(steamID64)',
  `ParentID` int(11) NULL DEFAULT NULL COMMENT '顶级评论ID，NULL表示顶级评论',
  `ReplyToPlatformID` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '被回复者平台ID',
  `Content` varchar(2000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '评论内容',
  `CreateTime` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '评论时间',
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0 COMMENT '是否已删除 0-未删除 1-已删除',
  PRIMARY KEY (`ID`) USING BTREE,
  INDEX `idx_pagepath`(`PagePath`) USING BTREE,
  INDEX `idx_parentid`(`ParentID`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '评论表' ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for CommentLike
-- ----------------------------
DROP TABLE IF EXISTS `CommentLike`;
CREATE TABLE `CommentLike`  (
  `ID` int(11) NOT NULL AUTO_INCREMENT,
  `CommentID` int(11) NOT NULL COMMENT '评论ID',
  `PlatformID` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '点赞者平台ID(steamID64)',
  `CreateTime` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '点赞时间',
  PRIMARY KEY (`ID`) USING BTREE,
  UNIQUE INDEX `uk_comment_user`(`CommentID`, `PlatformID`) USING BTREE,
  INDEX `idx_commentid`(`CommentID`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '评论点赞表' ROW_FORMAT = DYNAMIC;

-- 消息通知表
CREATE TABLE IF NOT EXISTS `Notification` (
    `ID` INT AUTO_INCREMENT PRIMARY KEY,
    `ReceiverID` VARCHAR(50) NOT NULL COMMENT '接收用户PlatformID',
    `SenderID` VARCHAR(50) NOT NULL COMMENT '发送用户PlatformID',
    `SenderName` VARCHAR(100) NOT NULL COMMENT '发送用户名称',
    `SenderAvatar` VARCHAR(500) COMMENT '发送用户头像',
    `Type` TINYINT NOT NULL COMMENT '消息类型：1-回复我 2-收到的点赞',
    `ContentID` INT NOT NULL COMMENT '关联内容ID（如评论ID）',
    `ContentType` VARCHAR(50) COMMENT '关联内容类型（如page）',
    `ContentTitle` VARCHAR(200) COMMENT '关联内容标题/摘要',
    `Message` VARCHAR(500) COMMENT '消息内容摘要',
    `IsRead` TINYINT(1) DEFAULT 0 COMMENT '是否已读：0-未读 1-已读',
    `CreateTime` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
    `ReadTime` DATETIME COMMENT '阅读时间',
    INDEX `idx_receiver` (`ReceiverID`),
    INDEX `idx_receiver_type` (`ReceiverID`, `Type`),
    INDEX `idx_receiver_read` (`ReceiverID`, `IsRead`),
    INDEX `idx_create_time` (`CreateTime`),
    INDEX `idx_unique_check` (`ReceiverID`, `SenderID`, `Type`, `ContentID`, `CreateTime`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='消息通知表';

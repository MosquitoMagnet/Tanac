#!/usr/bin/python3
# -*- coding: UTF-8 -*-

from os.path import expanduser

#调试专用
TERMINAL_UID_FOR_DEBUG = "HSAA004459"

#默认保存工单的文件，路径和文件名可被修改
DEFAULT_WOC_FILE = expanduser("~") + "/.config/mes/workorderCode.csv"

#默认保存报工单号的文件，路径和文件名可被修改
DEFAULT_WID_FILE = expanduser("~") + "/.config/mes/workorderReportId.csv"



#0: HOST_DEMO 
#1: HOST_SIT
#2: HOST_UAT 
#3: HOST_PRD

#调试时可以选择0：接口验证 or 1：SIT环境验证
HOST = 0

#调试时设为True
USE_DEBUG_TERMINAL = True

import os
import datetime
import time
from BET_LOG import Util_static
from BET_LOG import Starter
from BET_LOG import Stopper
import re
import pyodbc
from collections import defaultdict
import json


class Idle_Time:
    # 标准的idle time字典
    __idle_time_std_dic = {"LKF_sta": "",
                           "cell": "",
                           "stopper_time": datetime.datetime.now(),
                           "stopper_SN": "",
                           "error_code": 0,
                           "starter_time": datetime.datetime.now(),
                           "starter_SN": "",
                           "idle_time": 0
                           }
    # station, __log_file_dir,
    __lkf_station = ""
    __log_file_dir = ""

    __starter_log_dic = []
    __stopper_log_dic = []

    # 二维数据字典list， cell_log_array_dic, 每个cell的starter.log, stopper.log 记录都存储在 相对应的dic[cell_name]里面
    __cell_log_original_dic_array = defaultdict(list)

    # 有效的cell 数据list
    __cell_log_valid_dic_array = defaultdict(list)

    # 保存最后一行为stopper记录的数据文件
    __last_line_is_stopper_file_name = "last_line.log"

    # 初始化变量
    def __init__(self, _lkf_station, _log_file_dir):
        # 初始化所有变量
        self.__lkf_station = _lkf_station
        self.__log_file_dir = _log_file_dir
        self.__starter_log_dic.clear()
        self.__stopper_log_dic.clear()
        self.__cell_log_original_dic_array.clear()
        self.__cell_log_valid_dic_array.clear()

        #  ###################调试数据#############################################
        ss_log_time_start = time.time()
        print(_lkf_station + "       Idle_time init")
        #  ——————————————————————————————————————————————————————————————————————

        #  初始化starter.log  stopper.log
        self.__starter_log_dic = Starter.Starter_Log(_lkf_station, _log_file_dir).insert_log_database()
        self.__stopper_log_dic = Stopper.Stopper_log(_lkf_station, _log_file_dir).insert_log_database()

        #  ###################调试数据#############################################
        ss_log_time_end = time.time()
        #  ——————————————————————————————————————————————————————————————————————

        #  ###################调试数据#############################################
        print("starter_stopper_log read start time:    " + str(ss_log_time_start))
        print("starter_log_len:   " + str(len(self.__starter_log_dic)))
        print("stopper_log_len:   " + str(len(self.__stopper_log_dic)))
        print("starter_stopper_log read end   time:    " + str(ss_log_time_end))
        print("Total Starter.log Stopper.log Init time:" + str(ss_log_time_end-ss_log_time_start))
        #  ——————————————————————————————————————————————————————————————————————

        #  ##################调试数据#############################################
        #  将 starter_log_dic log list写入到文件中保存
        Util_static.Util.remove_single_file(self.__log_file_dir + "starter_log_dic.txt")
        for each in self.__starter_log_dic:
            Util_static.Util.append_line_to_file(self.__log_file_dir + "starter_log_dic.txt", str(each), "\n")
        #  ——————————————————————————————————————————————————————————————————————

        #  ###################调试数据#############################################
        #  将 stopper_log_dic log list写入到文件中保存
        Util_static.Util.remove_single_file(self.__log_file_dir + "stopper_log_dic.txt")
        for each in self.__stopper_log_dic:
            Util_static.Util.append_line_to_file(self.__log_file_dir + "stopper_log_dic.txt", str(each), "\n")
        #  ——————————————————————————————————————————————————————————————————————

        # 初始化cell 二维数据字典：分为两种，一种为FOF/FRIT  一种为Gemini
        station_row = 0
        station_col = 0
        if "FOF" in self.__lkf_station or "FRIT" in self.__lkf_station:
            station_row = 8
            station_col = 6

        if "Gemini" in self.__lkf_station:
            station_row = 22
            station_col = 12

        for i in range(1, station_row + 1):
            for j in range(1, station_col + 1):
                for k in range(0, 2):
                    self.__cell_log_original_dic_array[Util_static.Util.format_cell_name(i, j, k)] = []
                    self.__cell_log_valid_dic_array[Util_static.Util.format_cell_name(i, j, k)] = []

        # # 初始化上次记录的last line dic 将他们加到
        last_line_list = Util_static.Util.get_all_lines_from_file(
            self.__log_file_dir + self.__last_line_is_stopper_file_name)

        #  把上次的 stopper last line值加在该cell的最前端。
        for each in last_line_list:
            tmp_dic = eval(each)
            self.__cell_log_original_dic_array[tmp_dic["cell"]].append(tmp_dic)

        # 读取last_line 数据后删除文档
        Util_static.Util.remove_single_file(self.__log_file_dir + self.__last_line_is_stopper_file_name)

    def __sort_and_merge_log(self):
        tmp_merge_list = []
        for each in self.__starter_log_dic:
            tmp_dic = {key: each[key] for key in ['Log_time', 'cell', 'SN', 'log_type']}
            # starter.log 中有的log_type为2，那种非正常停止的start,在Idle统计时全部调整为1
            tmp_dic["log_type"] = 1
            tmp_merge_list.append(tmp_dic)

        for each in self.__stopper_log_dic:
            tmp_dic = {key: each[key] for key in ['Log_time', 'cell', 'SN', 'error_code', 'log_type']}
            tmp_merge_list.append(tmp_dic)

        # 重新按照时间排序
        tmp_merge_list.sort(key=lambda x: x["Log_time"])

        # 按照cell名称保存log 记录到 self.__cell_log_original_dic_array[cell] 的list中去
        for each in tmp_merge_list:
            self.__cell_log_original_dic_array[each["cell"]].append(each)

        # ========================debug 数据 orgin_dic.txt 开始 ==================
        Util_static.Util.remove_single_file(self.__log_file_dir + "orgin_dic.txt")
        for key in self.__cell_log_original_dic_array.keys():
            for each in self.__cell_log_original_dic_array[key]:
                Util_static.Util.append_line_to_file(self.__log_file_dir + "orgin_dic.txt", str(each), "\n")
        #  ——————————————————————————————————————————————————————————————————————

    # 原始的cell list数据中获取有效的cell list数据
    def get_valid_dic_array(self):
        # 读取log档案
        self.__sort_and_merge_log()
        for each in self.__cell_log_original_dic_array.keys():

            # 当前cell 的 log list
            current_cell_log_list = self.__cell_log_original_dic_array[each]

            # stopper flag, 默认 -1 为无效标识
            stopper_flag_index = -1
            starter_flag_index = -1

            # 有效数据flag
            valid_flag = False

            # for var
            i = 0
            while i < len(current_cell_log_list) - 1:
                # 重新新的一轮计算
                if current_cell_log_list[i]["log_type"] == 2:
                    stopper_flag_index = i
                    starter_flag_index = -1
                #  如果是Starter记录，
                else:
                    # 前面不存在stopper的记录，直接丢弃
                    if stopper_flag_index == -1:
                        i = i + 1
                        continue
                    # 如果存在stopper的记录，判断Starter和Stopper SN是否相同，不相同直接丢弃
                    if current_cell_log_list[i]["SN"] == current_cell_log_list[stopper_flag_index]["SN"]:
                        stopper_flag_index = -1
                        i = i + 1
                        continue
                    # 如果SN不同，并且starter的上一条记录就是stopper 此时为有效记录
                    if current_cell_log_list[i]["SN"] != current_cell_log_list[stopper_flag_index][
                        "SN"] and i == stopper_flag_index + 1:
                        self.__cell_log_valid_dic_array[each].append(current_cell_log_list[stopper_flag_index])
                        self.__cell_log_valid_dic_array[each].append(current_cell_log_list[i])
                        stopper_flag_index = -1
                        i = i + 1
                        continue

                i = i + 1
        # ========================debug 数据 valid_dic.txt 开始 ==================
        Util_static.Util.remove_single_file(self.__log_file_dir + "valid_dic.txt")
        for key in self.__cell_log_valid_dic_array.keys():
            for each in self.__cell_log_valid_dic_array[key]:
                Util_static.Util.append_line_to_file(self.__log_file_dir + "valid_dic.txt", str(each), "\n")
        #  ——————————————————————————————————————————————————————————————————————

        #  如果最后一条是stopper记录，就需要保存到文档中，以备下次使用
        for key in self.__cell_log_original_dic_array.keys():
            if len(self.__cell_log_original_dic_array[key]) != 0 and self.__cell_log_original_dic_array[key][-1]["log_type"] == 2:
                Util_static.Util.append_line_to_file(self.__log_file_dir + self.__last_line_is_stopper_file_name,
                                                     str(self.__cell_log_original_dic_array[key][-1]), "\n")

        return self.__cell_log_valid_dic_array

    # 插入idle time 有效值到数据库
    def insert_valid_log_to_database(self):
        # ###################################################################
        all_analysis_file_start = time.time()
        valid_dic_array = self.get_valid_dic_array()

        # #########################调试数据####################################
        all_analysis_end = time.time()
        print(self.__lkf_station + "---------------------Total analysis time:    " + str(all_analysis_end-all_analysis_file_start))
        #  ——————————————————————————————————————————————————————————————————

        # print(type(valid_dic_array))
        # print(len(valid_dic_array))

        insert_list = []

        for each_cell in valid_dic_array.keys():
            # print(each_cell + "   " + str(len(valid_dic_array[each_cell])))
            i = 0
            while i < len(valid_dic_array[each_cell]):

                tmp_tuple = (self.__lkf_station,
                             valid_dic_array[each_cell][i]["cell"],
                             Util_static.Util.convert_to_datetime(valid_dic_array[each_cell][i]["Log_time"]),
                             valid_dic_array[each_cell][i]["SN"],
                             valid_dic_array[each_cell][i]["error_code"],
                             Util_static.Util.convert_to_datetime(valid_dic_array[each_cell][i+1]["Log_time"]),
                             valid_dic_array[each_cell][i + 1]["SN"])
                insert_list.append(tmp_tuple)
                # print(tmp_tuple)
                i = i + 2
        if len(insert_list) == 0:
            return
            
        conn = Util_static.Util.get_conn()
        cursor = conn.cursor()
        start_time = time.time()
        insert_query = 'INSERT into Idle_time(lkf_sta, cell, stopper_time, stopper_SN , Error_code, starter_time, Starter_SN) values(?,?,?,?,?,?,?)'
        cursor.executemany(insert_query, insert_list)
        conn.commit()
        conn.close()

        # #########################调试数据####################################
        end_time = time.time()
        print(self.__lkf_station + "    insert count:   " + str(len(insert_list)) + "   insert time:   " + str(end_time-start_time))
        print("——————————————————————————————————————————————————————————————————")
        print("")
        #  ——————————————————————————————————————————————————————————————————




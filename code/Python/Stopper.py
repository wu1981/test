# starter.log class

import io
import os
import datetime
from BET_LOG import Util_static
import re


class Stopper_log:
    # log file dir
    __log_file_dir = ""

    # stopper.log file 默认的文件名.
    __log_file_name = 'Stopper.log'
    __log_file_name_1 = "Stopper.log.1"
    __log_file_name_2 = "Stopper.log.2"

    # 最后一次同步记录时间
    __last_log_time_file = "last_stopper.log"

    __lkf_station = ""

    # stopper_log_type 为2
    __log_time = 2
    # 标准的stopper log 字典结构
    __stopper_log_dic_std = {"Log_time": datetime.datetime.now(),
                             "SN": "",
                             "LKF_sta": __lkf_station,
                             "error_code": 0,
                             "cell": "",
                             "CM-cell-Tray": "",
                             "log_type": __log_time
                             }

    # 原始的string list，
    __stopper_log_list = []

    # 有效的log list 字典
    __stopper_log_dic_list = []

    # 初始化log file 对象, station, 和目录路径
    def __init__(self, _lkf_station, _log_file_dir):
        self.__log_file_dir = _log_file_dir
        self.__stopper_log_dic_std["LKF_sta"] = _lkf_station
        print(_lkf_station + "       stopper_time init")
        self.__stopper_log_list.clear()
        self.__stopper_log_dic_list.clear()

    #  读取有效字典到 __stopper_log_dic_list
    def get_log_dic_list(self):
        # 读取三个stopper.log
        tmp_stopper_log_list = Util_static.Util.get_all_lines_from_file(self.__log_file_dir + self.__log_file_name)
        tmp_stopper_log_1_list = Util_static.Util.get_all_lines_from_file(self.__log_file_dir + self.__log_file_name_1)
        tmp_stopper_log_2_list = Util_static.Util.get_all_lines_from_file(self.__log_file_dir + self.__log_file_name_2)
        # ############################初次读取数据使用 #######################
        # tmp_stopper_log_3_list = Util_static.Util.get_all_lines_from_file(self.__log_file_dir + "Stopper.log.3")
        # tmp_stopper_log_4_list = Util_static.Util.get_all_lines_from_file(self.__log_file_dir + "Stopper.log.4")
        # tmp_stopper_log_5_list = Util_static.Util.get_all_lines_from_file(self.__log_file_dir + "Stopper.log.5")
        # ############################初次读取数据使用 #######################
        """
               对于 stopper.log 的处理方法比较特殊一点，
               1.先将stopper.log, stopper.log.1, stopper.log.2 合并成为一个list self.stopper_log_list
               2.再用正则匹配出有用的行，存放在一个如下结构的字典list中 {“log_time”:date,"log_string":"ddd"}
               3.对上述字典进行以log_time排序
               4.看是否有存在last_record的log档案，如果不存在，返回所有的，如果存在，返回时间节点以后的。

        """
        print("__stopper_log_list  length:     " + str(len(self.__stopper_log_list)))
        # 1.先将stopper.log, stopper.log.1, stopper.log.2 合并成为一个list self.stopper_log_list
        # ############################初次读取数据使用 #######################
        # self.__stopper_log_list.extend(tmp_stopper_log_5_list)
        # self.__stopper_log_list.extend(tmp_stopper_log_4_list)
        # self.__stopper_log_list.extend(tmp_stopper_log_3_list)
        # ############################初次读取数据使用 #######################

        self.__stopper_log_list.extend(tmp_stopper_log_2_list)
        self.__stopper_log_list.extend(tmp_stopper_log_1_list)
        self.__stopper_log_list.extend(tmp_stopper_log_list)
        print("__stopper_log_list  length:     " + str(len(self.__stopper_log_list)))

        # 2.再用正则匹配出有用的行，存放在一个如下结构的字典list中 {“log_time”:date,"log_string":"ddd"}
        # tmp_dic_std 排序用的临时字典标准结构
        tmp_dic_std = {"log_time": datetime.datetime.now(),
                       "log_string": ""
                       }

        # tmp_valid_string_dic_list 本次操作的有效记录的字典, log_time, log_string： 每一行记录的字符串
        tmp_valid_string_dic_list = []

        # 符合要求的正则表达式，匹配出 出现error code的情况 Abort Test Complete | Stopped
        pattern = r'^\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2},\d{3}: ((Abort Test Complete)|(Stopped))'
        for each in self.__stopper_log_list:
            match = re.search(pattern, each)
            if match:
                tmp_dic = tmp_dic_std.copy()
                tmp_dic["log_time"] = Util_static.Util.convert_to_datetime(
                    Util_static.Util.get_log_time_from_string(each))
                tmp_dic["log_string"] = each
                tmp_valid_string_dic_list.append(tmp_dic)

        # 3.对上述字典进行以log_time排序.{"logtime":  , "log_string"  }
        tmp_valid_string_dic_list.sort(key=lambda x: x["log_time"])

        # 创建标准字典副本
        tmp_stopper_dic = self.__stopper_log_dic_std.copy()

        # 判断最后一次log记录档案的标志文件是否存在，
        # 如果不存在，所有的log都需要读取到字典中去
        if not (os.path.exists(self.__log_file_dir + self.__last_log_time_file)):
            for each in tmp_valid_string_dic_list:
                tmp_stopper_dic["Log_time"] = Util_static.Util.get_log_time_from_string(each["log_string"])
                tmp_stopper_dic["SN"] = Util_static.Util.get_SN_from_string(each["log_string"])
                tmp_stopper_dic["error_code"] = Util_static.Util.get_error_code_from_string(each["log_string"])
                tmp_stopper_dic["cell"] = Util_static.Util.get_tray_from_string(each["log_string"])
                tmp_stopper_dic["CM-cell-Tray"] = Util_static.Util.get_CM_tray_from_string(each["log_string"])
                self.__stopper_log_dic_list.append(tmp_stopper_dic.copy())
            # 将最后一条记录写在last_log_time_file 中去
            if len(tmp_valid_string_dic_list) > 0:
                Util_static.Util.write_line_to_file(self.__log_file_dir + self.__last_log_time_file,
                                                    (tmp_valid_string_dic_list[-1]["log_string"]))

        #  如果最后一条记录文件存在，
        if os.path.exists(self.__log_file_dir + self.__last_log_time_file):
            # 获取最后一次记录的string.
            tmp_last_log_string = Util_static.Util.get_first_line_from_file(self.__log_file_dir + self.__last_log_time_file)

            # 因为上述排序需要 tmp_valid_string_dic_list 是一个以 log_time, log_string 的字典结构
            # 需要重新删除["log_time"]，赋值给以log_string 的list 结构,找出匹配的index 再进行操作
            # tmp_valid_string_list

            tmp_valid_string_list = []
            # 转化为log_string的list 结构
            for each in tmp_valid_string_dic_list:
                tmp_valid_string_list.append(each["log_string"])

            # 获取最后一次last_log_time 在tmp_valid_string_list 的index 位置，然后从index之后进行操作
            index = len(tmp_valid_string_list) - tmp_valid_string_list[::-1].index(tmp_last_log_string) - 1

            for i in range(index + 1, len(tmp_valid_string_dic_list)):
                tmp_stopper_dic["Log_time"] = Util_static.Util.get_log_time_from_string(tmp_valid_string_list[i])
                tmp_stopper_dic["SN"] = Util_static.Util.get_SN_from_string(tmp_valid_string_list[i])
                tmp_stopper_dic["error_code"] = Util_static.Util.get_error_code_from_string(tmp_valid_string_list[i])
                tmp_stopper_dic["cell"] = Util_static.Util.get_tray_from_string(tmp_valid_string_list[i])
                tmp_stopper_dic["CM-cell-Tray"] = Util_static.Util.get_CM_tray_from_string(tmp_valid_string_list[i])
                self.__stopper_log_dic_list.append(tmp_stopper_dic.copy())

        # 将最后一条记录写在last_log_time_file 中去
            if index != len(tmp_valid_string_dic_list) - 1:
                Util_static.Util.write_line_to_file(self.__log_file_dir + self.__last_log_time_file,
                                                    (tmp_valid_string_dic_list[-1]["log_string"]))

        print(self.__lkf_station + "    Stopper Class log_lic length     :" + str(len(self.__stopper_log_dic_list)))

        return self.__stopper_log_dic_list

    '''
    将stopper.log 信息插入到数据库中去。
    '''
    def insert_log_database(self):

        insert_log_list = self.get_log_dic_list()

        # 如果没有有效记录返回
        if len(insert_log_list) == 0:
            return []

        conn = Util_static.Util.get_conn()
        cursor = conn.cursor()

        insert_list = []

        for each_dic in insert_log_list:
            tmp_tuple = (Util_static.Util.convert_to_datetime(each_dic["Log_time"]),
                         each_dic['LKF_sta'],
                         each_dic['cell'],
                         each_dic['SN'],
                         each_dic['error_code'],
                         each_dic['CM-cell-Tray'],
                         each_dic['log_type']
            )
            insert_list.append(tmp_tuple)
            # print(tmp_tuple)
            insert_query = 'insert into stopper(log_time,lkf_sta,cell,SN,error_code,cm_tray,log_type) values(?,?,?,?,?,?,?)'

        cursor.executemany(insert_query, insert_list)
        conn.commit()
        conn.close()
        print(self.__lkf_station + "   insert stopper log count:   " + str(len(insert_list)))

        return insert_log_list





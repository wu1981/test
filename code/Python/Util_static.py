# 该类为静态函数工具库，操作是直接调用Util.xxxx()

import io
from datetime import datetime, timedelta
import os.path
import re
import pyodbc


class Util:

    # 格式化时间函数. 返回3位数的毫秒  2023-09-23 15:00:22.638
    @staticmethod
    def format_datetime(_time: datetime):
        time_format_string = "%Y-%m-%d %H:%M:%S,%f"
        try:
            return _time.strftime(time_format_string)[:-3]
        except:
            return datetime.now().strftime(time_format_string)[:-3]

    # 将字符串 “2023-09-23 15:00:22.638” 转化成时间类型
    @staticmethod
    def convert_to_datetime(_time_string: str):
        time_format_string = "%Y-%m-%d %H:%M:%S,%f"
        return datetime.strptime(_time_string, time_format_string)
    
        # 将字符串 “2023-09-23 15:00:22.638” 转化成时间类型
    @staticmethod
    def convert_to_datetime_web(_time_string: str):
        time_format_string = "%d/%m/%y %H:%M"
        return datetime.strptime(_time_string, time_format_string)

    # 从字符传中获取时间
    @staticmethod
    def get_log_time_from_string(_log_string: str):
        time_pattern = r'^\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2},\d{3}'
        match = re.search(time_pattern, _log_string)
        if match:
            return match.group(0)
        else:
            return ""

    # 从字符传中获得SN
    @staticmethod
    def get_SN_from_string(_log_string: str):
        pattern = r'SN:\s*([A-Z0-9]{8})'
        match = re.search(pattern, _log_string)
        if match:
            return match.group(1)
        else:
            return ""

    # 从字符传中获得Partnum
    @staticmethod
    def get_Partnum_from_string(_log_string: str):
        pattern = r'Partnum:\s*([A-Z0-9]{6}-[0-9]{3})'
        match = re.search(pattern, _log_string)
        if match:
            return match.group(1)
        else:
            return ""

    # 获取SBR
    @staticmethod
    def get_SBR_from_string(_log_string: str):
        pattern = r'SBR:\s*([A-Z0-9]*)(,|\n)'
        match = re.search(pattern, _log_string)
        if match:
            return match.group(1)
        else:
            return ""

    # 获取Row-Col-Tray-Port: (04-04-01-0),
    @staticmethod
    def get_tray_from_string(_log_string: str):
        pattern = r'Row-Col-Tray-Port:\s*\((\d{2}-\d{2}-\d{2}-\d{1})\)'
        match = re.search(pattern, _log_string)
        if match:
            return match.group(1)
        else:
            return ""

    # 获取CM-Cell-Tray: (127.0.0.1-61-01),
    @staticmethod
    def get_CM_tray_from_string(_log_string: str):
        pattern = r'CM-Cell-Tray:\s*\((.*?)\)'
        match = re.search(pattern, _log_string)
        if match:
            return match.group(1)
        else:
            return ""

    # Product: EVANSBP,
    @staticmethod
    def get_Product_from_string(_log_string: str):
        pattern = r'Product:\s*([A-Z0-9]*)(,|$)'
        match = re.search(pattern, _log_string)
        if match:
            return match.group(1)
        else:
            return ""

    # get Eval: AUTO
    @staticmethod
    def get_Eval_from_string(_log_string: str):
        pattern = r'Eval:\s*(.*?)(,|$)'
        match = re.search(pattern, _log_string)
        if match:
            return match.group(1)
        else:
            return ""

    # Oper: SCR
    @staticmethod
    def get_Oper_from_string(_log_string: str):
        pattern = r'Oper:\s*(.*?)(,|$)'
        match = re.search(pattern, _log_string)
        if match:
            return match.group(1)
        else:
            return ""

    # Get Error Code: 10151
    @staticmethod
    def get_error_code_from_string(_log_string: str):
        pattern = r'Error Code:\s*(\d{5}|\d{1})(,|\s)'
        match = re.search(pattern, _log_string)
        if match:
            return match.group(1)
        else:
            return ""

    # Get read lines list from file

    # Get read lines list from file
    @staticmethod
    def get_all_lines_from_file(_file_name: str):
        if not os.path.exists(_file_name):
            return []
        tmp_file = open(_file_name, "r")
        tmp_lines = tmp_file.readlines()
        stripped_lines = [line.rstrip('\n') for line in tmp_lines]
        tmp_file.close()
        return stripped_lines

    # Get first line from file
    @staticmethod
    def get_first_line_from_file(_file_name: str):
        if not os.path.exists(_file_name):
            return []
        tmp_file = open(_file_name, "r")
        tmp_line = tmp_file.readline().rstrip('\n')
        tmp_file.close()
        return tmp_line

    # write line to file
    @staticmethod
    def write_line_to_file(_file_name: str, _string: str, enter_string=""):
        tmp_file = open(_file_name, "w")
        tmp_file.write(_string + enter_string)
        tmp_file.close()
        return True

    # write lines to file
    @staticmethod
    def write_lines_to_file(_file_name: str, _string: list, enter_string=""):
        tmp_file = open(_file_name, "w")
        for item in _string:
            tmp_file.write(str(item) + enter_string)
        tmp_file.close()
        return True

    # append line to file
    @staticmethod
    def append_line_to_file(_file_name: str, _string: str, enter_string=""):
        tmp_file = open(_file_name, "a")
        tmp_file.write(_string + enter_string)
        tmp_file.close()
        return True

    @staticmethod
    def get_conn():
        conn = pyodbc.connect(
            'Driver={SQL Server};'
            'Server=10.10.200.16;'
            'Database=BET_log;'
            'UID=sa;'
            'PWD=Aa789123;'
        )
        return conn

    # 格式化Row-Col-Tray-Port
    @staticmethod
    def format_cell_name(_row: int, _col: int, _port: int):
        cell_name = str(_row).zfill(2) + "-" + str(_col).zfill(2) + "-" + str(_port).zfill(2) + "-0"
        return cell_name

    # 删除文件
    @staticmethod
    def remove_single_file(filename: str):
        if os.path.exists(filename):
            os.remove(filename)

    # 删除文件
    @staticmethod
    def remove_file_list(filename_list: [], file_dir: str):
        for filename in filename_list:
            if os.path.exists(file_dir + filename):
                os.remove(file_dir + filename)

    # 获得server的config list
    @staticmethod
    def get_server_config_list(config_file, station_type=""):
        config_list = Util.get_all_lines_from_file(config_file)
        return_list = []
        for line in config_list:
            tmp = line.strip().split(" ")
            if station_type.lower() == "fof":
                if "fof" in tmp[0].lower():
                    return_list.append({'lkf_sta': tmp[0],
                                        'ip': tmp[1],
                                        'port': tmp[2],
                                        'user': tmp[3],
                                        'password': tmp[4]
                                        }
                                       )
            elif station_type.lower() == "gemini":
                if "gemini" in tmp[0].lower():
                    return_list.append({'lkf_sta': tmp[0],
                                        'ip': tmp[1],
                                        'port': tmp[2],
                                        'user': tmp[3],
                                        'password': tmp[4]
                                        }
                                       )
            elif station_type.lower() == "frit":
                if "frit" in tmp[0].lower():
                    return_list.append({'lkf_sta': tmp[0],
                                        'ip': tmp[1],
                                        'port': tmp[2],
                                        'user': tmp[3],
                                        'password': tmp[4]
                                        }
                                       )
            else:
                return_list.append({'lkf_sta': tmp[0],
                                    'ip': tmp[1],
                                    'port': tmp[2],
                                    'user': tmp[3],
                                    'password': tmp[4]
                                    }
                                   )

        return return_list

    # 初始化每个lkf_station 选中的时候select string，
    # 1代表已选中，0代表没有选中
    # 初始化字符为0或者，1两种，如果时1，11111111111111，如果是0  000000000000000000000
    # selected = True 返回 111， False返回0000
    @staticmethod
    def init_station_select_string(config_file, station_type="", selected=True):
        tmp_config_list = Util.get_server_config_list(config_file, station_type)
        if selected:
            return "1" * len(tmp_config_list)
        else:
            return "0" * len(tmp_config_list)

    # 取反selected string,
    @staticmethod
    def inverse_selected_string(selected_string: str):
        reverse_string = selected_string.replace("0", "X").replace("1", "0").replace("X", "1")
        return reverse_string

    # 获取需要查询的lkf_sta的list。就是为1的。
    @staticmethod
    def get_selected_lkf_sta_list_from_string(config_file: str, selected_string: str, station_type=""):
        selected_lkf_sta_list = []
        # 获取所有的config list
        config_list = Util.get_server_config_list(config_file, station_type)

        i = 0
        for i in range(0, len(selected_string)):
            if selected_string[i] == "1":
                selected_lkf_sta_list.append(config_list[i]["lkf_sta"])

        return selected_lkf_sta_list
    
        # 获取需要查询的lkf_sta的list。就是为1的。
    
    # 获取当前时间的前一天
    @staticmethod
    def get_sum_start_time_from_now():
        yesterday = datetime.now() - timedelta(days=1)
        return yesterday.replace(hour=0, minute=0, second=0, microsecond=0)
    
    # 获取当前时间的24点整
    @staticmethod
    def get_sum_end_time_from_now():
        return datetime.now().replace(hour=23, minute=59, second=59, microsecond=0)

    # 把字符串转化为字典格式，
    @staticmethod
    def convert_string_to_dic(_dic_string: str):
        return eval(_dic_string)

    # 获取文件的创建时间，
    @staticmethod
    def get_file_create_time(_file: str):
        if os.path.exists(_file):
            return datetime.fromtimestamp(os.path.getctime(_file))
        else:
            return None



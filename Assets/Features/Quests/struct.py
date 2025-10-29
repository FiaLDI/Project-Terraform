import os
import sys
from pathlib import Path

class ProjectAnalyzer:
    def __init__(self, root_dir):
        self.root_dir = os.path.abspath(root_dir)
        self.output_file = os.path.join(self.root_dir, "struct.txt")
        
        # Настройки фильтрации 
        self.ignored_dirs = set()
        self.ignored_files = set()
        self.allowed_extensions = set()
        
        # Настройки обработки файлов
        self.max_file_size = 10 * 1024 * 1024  # 10MB максимум
        self.encodings = ['utf-8', 'utf-8-sig', 'utf-16', 'windows-1251', 'cp1251', 'latin-1', 'iso-8859-1']
        
        self.original_stdout = sys.stdout
        self.file_output = open(self.output_file, 'w', encoding='utf-8')
        sys.stdout = DualOutput(self.original_stdout, self.file_output)

    def __del__(self):
        sys.stdout = self.original_stdout
        self.file_output.close()

    def analyze(self):
        print(f"Анализ проекта: {self.root_dir}\n")
        print("Структура проекта:")
        self._print_project_structure(self.root_dir)
        print("\nТекст из файлов проекта:\n")
        self._print_file_contents(self.root_dir)
        print(f"\nАнализ завершен. Результаты сохранены в {self.output_file}")

    def _print_project_structure(self, current_dir, indent="", is_last=True):
        relative_path = os.path.relpath(current_dir, self.root_dir)
        display_name = os.path.basename(self.root_dir) if relative_path == "." else os.path.basename(current_dir)
        
        branch = "└── " if is_last else "├── "
        print(f"{indent}{branch}{display_name}")
        
        indent += "    " if is_last else "│   "
        
        try:
            items = sorted(os.listdir(current_dir))
        except (PermissionError, FileNotFoundError):
            print(f"{indent}└── <нет доступа>")
            return

        dirs = [d for d in items 
               if os.path.isdir(os.path.join(current_dir, d)) and 
               d not in self.ignored_dirs]
        
        files = [f for f in items 
                if os.path.isfile(os.path.join(current_dir, f)) and
                f not in self.ignored_files]
        
        for i, dir_name in enumerate(dirs):
            dir_path = os.path.join(current_dir, dir_name)
            if not os.path.abspath(dir_path).startswith(self.root_dir):
                continue
            is_last_dir = (i == len(dirs) - 1) and (len(files) == 0)
            self._print_project_structure(dir_path, indent, is_last_dir)
        
        for i, file_name in enumerate(files):
            is_last_file = i == len(files) - 1
            branch = "└── " if is_last_file else "├── "
            print(f"{indent}{branch}{file_name}")

    def _is_binary_file(self, file_path):
        """Проверяет, является ли файл бинарным по наличию нулевых байтов"""
        try:
            with open(file_path, 'rb') as f:
                chunk = f.read(1024)
                if b'\0' in chunk:  # Наличие нулевых байтов - признак бинарного файла
                    return True
        except Exception:
            return True
        return False

    def _read_file_with_encodings(self, file_path):
        """Пытается прочитать файл с разными кодировками"""
        for encoding in self.encodings:
            try:
                with open(file_path, 'r', encoding=encoding) as f:
                    return f.read(), encoding
            except UnicodeDecodeError:
                continue
            except Exception as e:
                # Для других ошибок пробуем следующую кодировку
                continue
        return None, None

    def _read_file_as_text_fallback(self, file_path):
        """Альтернативный метод чтения файла с фильтрацией непечатаемых символов"""
        try:
            with open(file_path, 'rb') as f:
                binary_content = f.read()
            
            # Пробуем декодировать как UTF-8, игнорируя ошибки
            content = binary_content.decode('utf-8', errors='ignore')
            # Убираем нулевые символы и другие непечатаемые символы, кроме стандартных
            content = ''.join(char for char in content if ord(char) >= 32 or char in '\n\r\t')
            return content, 'binary_fallback'
        except Exception as e:
            return None, str(e)

    def _print_file_contents(self, root_dir):
        for root, dirs, files in os.walk(root_dir):
            dirs[:] = [d for d in dirs 
                      if d not in self.ignored_dirs and
                      os.path.abspath(os.path.join(root, d)).startswith(self.root_dir)]
            
            files[:] = [f for f in files 
                       if f not in self.ignored_files and
                       os.path.abspath(os.path.join(root, f)).startswith(self.root_dir)]
            
            for file in files:
                file_path = os.path.join(root, file)
                file_ext = os.path.splitext(file)[1].lower()
                if file_ext not in self.allowed_extensions:
                    continue
                
                # Проверяем размер файла
                try:
                    file_size = os.path.getsize(file_path)
                    if file_size > self.max_file_size:
                        print(f"\nСодержимое {file_path}:\n<файл слишком большой: {file_size} байт>")
                        continue
                    elif file_size == 0:
                        print(f"\nСодержимое {file_path}:\n<пустой файл>")
                        continue
                except OSError as e:
                    print(f"\nОшибка доступа к {file_path}: {str(e)}")
                    continue
                
                # Пытаемся прочитать файл
                content = None
                encoding_used = None
                error_message = None
                
                try:
                    # Сначала проверяем, не бинарный ли файл
                    if self._is_binary_file(file_path):
                        print(f"\nСодержимое {file_path}:\n<бинарный файл>")
                        continue
                    
                    # Пробуем разные текстовые кодировки
                    content, encoding_used = self._read_file_with_encodings(file_path)
                    
                    # Если текстовые кодировки не сработали, пробуем fallback метод
                    if content is None:
                        content, encoding_used = self._read_file_as_text_fallback(file_path)
                        if content is None:
                            error_message = f"<не удалось прочитать файл: {encoding_used}>"
                            encoding_used = None
                            
                except PermissionError:
                    error_message = "<нет прав доступа>"
                except Exception as e:
                    error_message = f"<ошибка: {str(e)}>"
                
                # Выводим результат
                if content is not None:
                    encoding_info = f" (кодировка: {encoding_used})" if encoding_used and encoding_used != 'binary_fallback' else ""
                    print(f"\nСодержимое {file_path}{encoding_info}:\n{content}")
                elif error_message:
                    print(f"\nСодержимое {file_path}:\n{error_message}")

class DualOutput:
    def __init__(self, stdout, file):
        self.stdout = stdout
        self.file = file

    def write(self, text):
        self.stdout.write(text)
        self.file.write(text)

    def flush(self):
        self.stdout.flush()
        self.file.flush()


if __name__ == "__main__":
    project_root = input("Введите путь к корневой директории проекта: ").strip() or os.getcwd()
    
    if not os.path.exists(project_root):
        print("Указанный путь не существует!")
        sys.exit(1)
    
    analyzer = ProjectAnalyzer(root_dir=project_root)
    
    # Настройки фильтрации
    analyzer.ignored_dirs = {'venv', '.git', '__pycache__', 'copy','.idea', 'node_modules', 'dist', 'build', 'Article'}
    analyzer.ignored_files = {'__init__.py', 'goidachat.py','giga.py', 'settings.py', 'struct.txt', 'LicescePage.tsx','PrivacyPage.tsx','struct.py','config.py', 'secret.py', 'credentials.json'}
    analyzer.allowed_extensions = {'.cs'}  
    
    # Можно настроить дополнительные параметры
    analyzer.max_file_size = 5 * 1024 * 1024  # 5MB максимум
    
    try:
        analyzer.analyze()
    except KeyboardInterrupt:
        print("\nАнализ прерван пользователем.")
    except Exception as e:
        print(f"\nПроизошла ошибка: {str(e)}")
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace T2_V2
{
    //The class responsible for making a 1M file 
    class Virtual_Disk
    {

        public static void Initialize()//Function to initialize the super block and the fatTable beside the data file
        {
          
            if (!File.Exists("Try.txt"))
            {
                //creating a txt file called "Try" or overwritting if it is already existing
                using (FileStream stream = new FileStream("Try.txt", FileMode.Create, FileAccess.ReadWrite))
                {
                    //Intializing Super Block into the first block block[0] byte by byte in the form of 1024 zero
                    for (int j = 0; j < 1024; j++)
                        stream.WriteByte((byte)'0');

                    //Intializing  fatTable into the next 4 blocks block[1,2,3,4] in the form of 4 blocks each contains only '*'
                    for (int i = 0; i < 4; i++)
                        for (int j = 0; j < 1024; j++)
                            stream.WriteByte((byte)'*');

                    //Intializing  the rest of the blocks form block [5 to 1023] with '#'
                    for (int i = 0; i < 1019; i++)
                        for (int j = 0; j < 1024; j++)
                            stream.WriteByte((byte)'#');

                    Fat_Table.Initailize();
                    Directory root = new Directory("H".ToCharArray(), 0x10, 5);
                    Fat_Table.set_Next(5, -1);
                    stream.Close();
                    root.write_Directory();
                    Fat_Table.write_Fat_Table();
                    Program.current_dir = root;
                }
            }

            else
            {
                Fat_Table.get_Fat_Table();
                Directory root = new Directory("H".ToCharArray(), 0x10, 5);
                if(Fat_Table.get_Next(5)!=0)
                root.Read_Directory();
                Program.current_dir = root;

            }


        }
        //function to write a block of bytes (1024 byte) into the file that 
        //takes 2 args : the wanted block to copy and the index of the block in the file to copy into 
        public static void write_Block(byte[] block, int index)
        {
            using (FileStream stream = new FileStream("Try.txt", FileMode.Open, FileAccess.ReadWrite))
            {

                stream.Seek(index * 1024, SeekOrigin.Begin);

                for (int i = 0; i < 1024; i++)
                    stream.WriteByte(block[i]);

            }

        }
        //function to get a specific block from the file :takes the index of the wanted block and returns it as array of bytes
        public static byte[] get_Block(int index)
        {
            using (FileStream stream = new FileStream("Try.txt", FileMode.Open, FileAccess.ReadWrite))
            {
                byte[] Readblock = new byte[1024];

                stream.Seek(1024 * index, SeekOrigin.Begin);
                stream.Read(Readblock, 0, 1024);

                return Readblock;

            }
        }
    }
    //End__________________________ of Virtual__________Disk____________________ Class

    class Fat_Table //class to write the fatTable and read it or print it or other things
    {
        //fatTable array contains 1024 intger intialized in the constructor below
        static int[] fatT = new int[1024];

        public static void Initailize()
        {

            //initializing the reserved places of the fatTable from fat[0 to 4] 
            //that points to the first 5 blocks in the file the superblock beside its own block    
            fatT[0] = -1; fatT[1] = 2; fatT[2] = 3; fatT[3] = 4; fatT[4] = -1;

            for (int i = 5; i < fatT.Length; i++)
                fatT[i] = 0;

        }
        //function to write the fatTable in 4 blocks in the file form block[1 to 4]
        public static void write_Fat_Table()
        {
            using (FileStream stream = new FileStream("Try.txt", FileMode.Open, FileAccess.Write))
            {
                //array of bytes (was sbyte if error happened return it to sbyte ) 
                //takes the values of the fatT array after converting it from intger into the equivalent bytes  
                byte[] fatClone = new byte[4096];

                //converting the fatTable intgers into bytes stored at fatCloen array
                Buffer.BlockCopy(fatT, 0, fatClone, 0, fatT.Length);

                //moving the writing cursor 1024 byte from the start of the file to skip the place of the superblock 
                stream.Seek(1024, SeekOrigin.Begin);

                //Writing the FatTable  in the form of bytes stored at fatClone
                for (int i = 0; i < fatClone.Length; i++)
                    stream.WriteByte(fatClone[i]);

                stream.Close();

            }
        }

        //function that reads the values of fatTable from the file and change the array of fatTable 
        //into the values from the file in case of any update happens .i dont have to deal with the
        //file the array at the beginning of the class is sufficient
        public static void get_Fat_Table()
        {
            using (FileStream stream = new FileStream("Try.txt", FileMode.Open, FileAccess.Read))
            {
                //Reading elemnts of FatTable from the file into the form of bytes then assigning it 
                //after converting into intgers into the above array at the start of the class
                byte[] ReadFat = new byte[4096];

                stream.Seek(1024, SeekOrigin.Begin);

                stream.Read(ReadFat, 0, 4096);
                //the function responsible of converting from array of bytes into array of intgers and vice versa
                Buffer.BlockCopy(ReadFat, 0, fatT, 0, 4096);

            }
        }
        //function to print the values of the fatTable into the console
        public static void print_Fat_Table()
        {
            get_Fat_Table();

            for (int i = 0; i < fatT.Length; i++)
                Console.Write($"[{i}] = {fatT[i]} \t");

        }

        //function that searches for available empty blocks to prepare for writing into that empty block maybe
        public static int get_available_block()
        {//if none is empty return the default -1
            int index = -1;
            //gets the first 0 (first empty index)to appear
            for (int i = 0; i < fatT.Length; i++)
            {
                if (fatT[i] == 0)
                {
                    index = i;
                    break;
                }
            }
            return index;
        }

        //function takes an index of the fatTable and returns its value
        public static int get_Next(int index)
        {
            return fatT[index];
        }

        //function sets a value to a specific index in the fatTable in case i was writing into block in the file and it wasn't enough
        //the fatTable then would point to the next block that i continued wrtitng in insted of -1
        public static void set_Next(int index, int value)
        {
            fatT[index] = value;
        }

        //function to get the number of availble empty blocks in the file
        public static int get_availbale_blocks()
        {
            int counter = 0;

            for (int i = 0; i < fatT.Length; i++)
            {
                if (fatT[i] == 0)
                    counter += 1;

            }

            return counter;
        }

        public static int get_free_space()
        {
            return get_availbale_blocks() * 1024;
        }

    }
    //_______Directory______________Entry____________Start_______________________________

    class Directory_Entry
    {
        public char[] name = new char[11];
        public byte attribute;
        byte[] empty = new byte[12];
        public  int size;
        public int first_cluster;

        public Directory_Entry() { }

        public Directory_Entry(char[] n, byte attri, int clust,int sz=0)
        {
        if (n.Length > 11)           
            for (int i = 0; i < 11; i++)
                name[i] = n[i];
            
          else
            name = n;

            attribute = attri;
            first_cluster = clust;
            size = sz;

        }

        public byte[] get_Bytes()
        {
            byte[] b = new byte[32];
            int[] intClone = new int[1];

            for (int i = 0; i < name.Length; i++)
                b[i] = (byte)name[i];

            b[11] = attribute;
            Buffer.BlockCopy(empty, 0, b, 12, empty.Length);

            intClone[0] = first_cluster;
            Buffer.BlockCopy(intClone, 0, b, 24, 1);

            intClone[0] = size;
            Buffer.BlockCopy(intClone, 0, b, 28, 1);

            return b;


        }

        public Directory_Entry get_Directory_Entry(byte[] b)
        {
            Directory_Entry d = new Directory_Entry();

            int[] intClone = new int[1];


            for (int i = 0; i < 11; i++)
                d.name[i] = (char)b[i];

            d.attribute = b[11];


            Buffer.BlockCopy(b, 12, d.empty, 0, d.empty.Length);

            Buffer.BlockCopy(b, 24, intClone, 0, 4);
            d.first_cluster = intClone[0];

            Buffer.BlockCopy(b, 28, intClone, 0, 4);
            d.size = intClone[0];

            return d;
        }
        public Directory_Entry get_Directory_Entry()
        {
            Directory_Entry d = new Directory_Entry();

            d.name = name;

            d.attribute = attribute;

            d.empty = empty;

            d.first_cluster = first_cluster;

            d.size = size;

            return d;
        }


    }
    //____________Start___________of____________class__________directory_________________________
    class Directory : Directory_Entry
    {

        public List<Directory_Entry> Directory_table = new List<Directory_Entry>();

        public Directory Parent;



        public Directory(char[] n, byte attri, int clust, Directory Par = null) : base(n, attri, clust)
        {

            Parent = Par;
        }

        public void write_Directory()
        {

            byte[] DTB = new byte[32 * Directory_table.Count];
            byte[] DEB = new byte[32];

            for (int i = 0; i < Directory_table.Count; i++)
            {

                DEB = Directory_table[i].get_Bytes();

                for (int j = i * 32, c = 0; c < 32; j++, c++)
                {
                    DTB[j] = DEB[c];
                }

            }

            double num_of_req_blocks = Math.Ceiling(DTB.Length / 1024.0);

            if (num_of_req_blocks <= Fat_Table.get_availbale_blocks())
            {
                int FI;
                int LI = -1;

                if (first_cluster != 0)
                    FI = first_cluster;

                else
                {

                    FI = Fat_Table.get_available_block();
                    first_cluster = FI;
                }
            
                byte[] clone = new byte[1024];
                if (num_of_req_blocks == 0)
                {
                    for (int i = 0; i < 1024; i++)
                        clone[i] = (byte)'*';
                    Virtual_Disk.write_Block(clone, FI);
                }


                for (int i = 0; i < num_of_req_blocks; i++)
                    {
                        for (int j = i * 1024, c = 0; c < 1024; j++, c++)
                        {

                            if (j < DTB.Length)
                                clone[c] = DTB[j];

                            else
                                clone[c] = (byte)'*';

                        }

                        Virtual_Disk.write_Block(clone, FI);
                        Fat_Table.set_Next(FI, -1);

                        if (LI != -1)
                            Fat_Table.set_Next(LI, FI);


                        LI = FI;
                        //  Console.WriteLine("LI :"+LI);
                        FI = Fat_Table.get_available_block();
                        //  Console.WriteLine("FI :"+FI);

                    }
                

                Fat_Table.write_Fat_Table();
            }


        }

        public void Read_Directory()
        {
            List<byte> ls = new List<byte>();
            List<Directory_Entry> DT = new List<Directory_Entry>();

            int FI;
            int Next;

            if (first_cluster != 0)
            {
                FI = first_cluster;
                Next = Fat_Table.get_Next(FI);
                ls.AddRange(Virtual_Disk.get_Block(FI));

                do
                {
                    // ls.AddRange(Virtual_Disk.get_Block(FI));
                    FI = Next;
                    if (FI != -1)
                    {
                        ls.AddRange(Virtual_Disk.get_Block(FI));
                        Next = Fat_Table.get_Next(FI);
                    }

                } while (Next != -1);


                byte[] clone2 = new byte[32];

                for (int i = 0; i < ls.Count;)
                {

                    for (int j = 0; j < 32; j++, i++)
                    {

                        clone2[j] = ls[i];

                    }


                    DT.Add(get_Directory_Entry(clone2));
                }

            }
            Directory_table = DT;

        }

        public int Search(string s)
        {
            s = s.TrimEnd(new char[] { '\0' });
            string str;

            for (int i = 0; i < Directory_table.Count; i++)
            {

                str = new string(Directory_table[i].name).TrimEnd(new char[] { '\0' });

               //    Console.WriteLine(s);
              //  Console.WriteLine(str);

                if (str == s)
                    return i;
                
            }

            return -1;

        }

        public void Update_Content(Directory_Entry d)
        {

            string file_name = new string(d.name);

            Read_Directory();

            int index = Search(file_name);
            if (index != -1)
            {
                Directory_table.RemoveAt(index);
                Directory_table.Insert(index, d);
               // Console.WriteLine(d.first_cluster+"aaaaaaaaaa");
            }
            write_Directory();
        }

        public void delete_Directory()
        {
            if (first_cluster != 0)
            {
                int index = first_cluster;
                int next = Fat_Table.get_Next(index);

                do
                {
                    Fat_Table.set_Next(index, 0);
                    index = next;
                    if (index != -1)
                        next = Fat_Table.get_Next(index);

                } while (next != -1);

                Fat_Table.write_Fat_Table();
            }
            if (Parent != null)
                {
                    string file_name = new string(name);
                    Parent.Read_Directory();
                    int i = Parent.Search(file_name);

                    if (i != -1)
                    {
                        Parent.Directory_table.RemoveAt(i);
                        Parent.write_Directory();
                    }
                }

        }

    }
    //____________Start___________of____________class__________File______________Entry___________
    class File_Entry : Directory_Entry
    {
        public string content;
        Directory Parent;


        public File_Entry(char[] n, byte attri, int clust,int siz,string cont ="" ,Directory Par = null) : base(n, attri, clust,siz)
        {
            Parent = Par;
            content = cont;
        }

        public void write_file()
        {

            double num_of_req_blocks = Math.Ceiling(content.Length / 1024.0);


            if (num_of_req_blocks <= Fat_Table.get_availbale_blocks())
            {
                int FI;
                int LI = -1;

                if (first_cluster != 0)
                    FI = first_cluster;

                else
                {
                    FI = Fat_Table.get_available_block();
                    first_cluster = FI;
                }


                byte[] clone = new byte[1024];
                for (int i = 0; i < num_of_req_blocks; i++)
                {
                    for (int j = i * 1024, c = 0; c < 1024; j++, c++)
                    {

                        if (j < content.Length)
                            clone[c] = (byte)content[j];

                        else
                            clone[c] = (byte)'*';

                    }


                    Virtual_Disk.write_Block(clone, FI);
                    Fat_Table.set_Next(FI, -1);

                    if (LI != -1)
                        Fat_Table.set_Next(LI, FI);

                    LI = FI;

                    FI = Fat_Table.get_available_block();

                }
                Fat_Table.write_Fat_Table();
            }
        }
        public void read_file()
        {
            List<byte> ls = new List<byte>();

            int FI;
            int Next;

            if (first_cluster != 0)
            {
                FI = first_cluster;
                Next = Fat_Table.get_Next(FI);
                ls.AddRange(Virtual_Disk.get_Block(FI));

                do
                {
                    FI = Next;
                    if (FI != -1)
                    {
                        ls.AddRange(Virtual_Disk.get_Block(FI));
                        Next = Fat_Table.get_Next(FI);
                    }

                } while (Next != -1);



                for (int i = 0; i < ls.Count; i++)
                {
                    content += (char)ls[i];

                }

            }

        }

        public void delete_file()
        {
            if (first_cluster != 0)
            {
                int index = first_cluster;
                int next = Fat_Table.get_Next(index);

                do
                {
                    Fat_Table.set_Next(index, 0);
                    index = next;
                    if (index != -1)
                        next = Fat_Table.get_Next(index);

                } while (next != -1);

                Fat_Table.write_Fat_Table();
            }
            if (Parent != null)
            {
                string file_name = new string(name);
                Parent.Read_Directory();
                int i = Parent.Search(file_name);

                if (i != -1)
                {
                    Parent.Directory_table.RemoveAt(i);
                    Parent.write_Directory();
                    
                }
            }

        }


    }

    class Cmd
    {
        public static string CurrentDirectory;
        public static void Initialize()
        {
            Console.WriteLine("Microsoft Windows [Version 6.1.7601]" +
                "\nCopyright <c> 2009 Microsoft Coporation. All rights reserved.\n");
            //   Console.WriteLine("        amr       ".Trim());

            CurrentDirectory ="H:";
            string import_path = "";
            string export_path = "";
            while (true) //takes commands until exit
            {

            //    CurrentDirectory = System.IO.Directory.GetCurrentDirectory();       //get current .exe directory
                Console.Write(CurrentDirectory + ">");                       //display the const line of current directory
                var command = Console.ReadLine();

                if(command.Length>6)
                 import_path = command.Substring(7); 
                 
                                                            //take command from user with or without argument
                    //     command = command.ToLower();    //convert command to lowercase to compare it with my commands later
                                                                           //array of available commands
                var commands = new[] { "cls", "help", "exit", "cd", "dir", "copy", "del", "md", "rd", "rename", "type", "import","export" };
                var valid = false;                //flag to check if the enterd command exists in my array of commands
                char[] whitespace = new char[] { ' ', '\t' };
                var indexe = command.Split(whitespace,StringSplitOptions.RemoveEmptyEntries);
                if(indexe.Length>0)
               indexe[0] = indexe[0].ToLower();
                                                                                                     /*split enterd command from it 's argument if existed 
                                                                                                 into indexe[0] for the command and indexe[1]for the argument
                                                                                                 and remove any white spaces between*/

               //    Console.WriteLine(indexe[0]+" "+ indexe[1]); //note for me 
                if(import_path.Length >0 && indexe.Length>2)
                 export_path = import_path.Substring(indexe[1].Length+1);

               // Console.WriteLine(indexe[1]);
                //loop to check if the enterd command valid and exists in my array or not
                if (indexe.Length != 0)
                {
                    foreach (var c in commands)
                    {
                        if (indexe[0] == c)
                        {

                            valid = true;
                            break;
                        }

                        else
                            valid = false;
                    }//end of first foreach loop

                    if (valid)//configuring which command is enterd as it 's valid and confirmed to be in my array 
                    {
                        bool wrongValue = true;//for help arguments

                        if (command == "cls")//clearing the console
                        {
                            Console.Clear();
                            Console.Write("\n");

                        }
                        else if (command == "exit")//exiting the program                  
                            Environment.Exit(0);

                        else if (indexe[0] == "help")
                        {

                            string argument;//argument of help 
                            if (command == indexe[0] || string.IsNullOrWhiteSpace(command.Remove(0, 4)))//First case
                                Commands.helpAlone();//excuted only if the whole command = help or command = help + white spaces

                            else if (command != indexe[0])//Second case there is an argument 
                            {

                                foreach (var c in commands)//loops on every command in my array 
                                {
                                    if (indexe[1] == c)//see if the argument of help exsits at my array of commands or not
                                    {
                                        wrongValue = false;//argument is correct and exists
                                        argument = c;
                                        Commands.helpArg(argument);//perform the function located at class help and get help defintion for the argument
                                        break;//end the loop first time the condition above is correct
                                    }

                                    else
                                        wrongValue = true;//argumet is not correct

                                }

                                if (wrongValue)//argumet is not valid so it prints that 
                                    Console.WriteLine("This command is not supported by the help utility.\n");


                            }

                        }

                        else if (indexe[0] == "md" && indexe.Length == 2)
                        {
                            Commands.md(indexe[1]);
                            Console.WriteLine();
                        }
                        else if (indexe[0] == "rd" && indexe.Length == 2)
                        {
                            Commands.rd(indexe[1]);
                            Console.WriteLine();
                        }

                      
                        else if (indexe[0] == "cd" && indexe.Length == 1)
                        {
                            Console.WriteLine(CurrentDirectory+"\n");
                        }

                        else if (indexe[0] == "cd" && indexe.Length == 2 &&indexe[1]!="..")
                        {
                           Commands.cd(indexe[1]);
                        }

                        else if (indexe[0] == "dir" && indexe.Length == 1)
                        {
                            Commands.dir();
                        }

                        else if (indexe[0] == "import")
                        {
                         
                         //   Console.WriteLine(import_path);
                            Commands.import(import_path);
                        }

                        else if (indexe[0] == "type" && indexe.Length == 2)
                        {

                            Commands.type(indexe[1]);
                        }

                        else if (indexe[0] == "export")
                        {

                            Commands.export(indexe[1],export_path);
                        }

                        else if (indexe[0] == "rename" && indexe.Length ==3)
                        {

                            Commands.rename(indexe[1], indexe[2]);
                        }

                        else if (indexe[0] == "del" && indexe.Length == 2)
                        {
                            Commands.del(indexe[1]);
                        }

                        else if (indexe[0] == "copy" && indexe.Length == 3)
                        {
                            Commands.copy(indexe[1],indexe[2]);
                        }


                        else if (indexe[0] == "cd" &&indexe[1]==".."&&indexe.Length == 2)
                        {
                            Commands.cdPrev(indexe[1]);
                        }



                        else
                        {
                            Console.WriteLine("\nThe syntax of the command is incorrect.\n");
                        }

                    }

                    //else for the if of valid says that this command is not in my array 
                    else
                        Console.WriteLine("\'" + command + "\' " +
                            "is not registerd as an internal or external command," +
                               "\noperable program or batch file.\n");
                }



            }


        }

    }

    class Commands
    {

        public static void helpAlone()//help without argument
        {
            string[] lines = File.ReadAllLines("help.txt");//reads from file in the form of array of lines of strings
            foreach (string line in lines) //looping on the array of lines 
                Console.WriteLine(line);//printing each line 

        }

        public static void helpArg(string argum)//help with argument takes an argument string as parameter
        {
            string[] lines = File.ReadAllLines("help.txt");//read from txt file into array lines of strings
            foreach (string line in lines)
            {//looping on each line
                if (line.Contains(argum.ToUpper()))//compares each line with the argument entered until it finds it 
                {
                    Console.WriteLine("\n");//print embty line
                    Console.WriteLine(line + "\n");//print the line that contains the argument then print embty line after
                    break;//terminate the loop
                }

            }
        }


        public static void md(string name)
        {
           // Fat_Table.Initailize();
            int index = Program.current_dir.Search(name);

            if (index == -1)
            {
                Directory_Entry d = new Directory_Entry(name.ToCharArray(), 0x10, 0);
                Program.current_dir.Directory_table.Add(d);
                Program.current_dir.write_Directory();

                if (Program.current_dir.Parent != null)
                    Program.current_dir.Parent.Update_Content(Program.current_dir.get_Directory_Entry());
            }

            else
            {
                Console.WriteLine($"\nA subdirectory or file {name} already exists.");
            }
        }

        public static void rd(string name)
        {
            int index = Program.current_dir.Search(name);
            if (index != -1 && Program.current_dir.Directory_table[index].attribute == 0x10)
            {
               
                    int Fc = Program.current_dir.Directory_table[index].first_cluster;
                    Directory d = new Directory(name.ToCharArray(), 0x10, Fc, Program.current_dir);
                    d.delete_Directory();
                
            }
            else
                Console.WriteLine("\nThe system cannot find the directory specified.");

        }

        public static void cd(string name)
        {
            int index = Program.current_dir.Search(name);

            if (index != -1 && Program.current_dir.Directory_table[index].attribute == 0x10)
            {
                int Fc = Program.current_dir.Directory_table[index].first_cluster;
                Directory d = new Directory(name.ToCharArray(), 0x10, Fc, Program.current_dir);
                Program.current_dir = d;
                string str = new string(d.name).TrimEnd(new char[] { '\0' });
                Program.current_path = "\\" + str;
                Program.current_dir.Read_Directory();
                Cmd.CurrentDirectory += Program.current_path;
                Console.WriteLine();

            }

            else
            {
                Console.WriteLine("\nThe system cannot find the path specified.\n");
            }

        }

        public static void cdPrev(string dots)
        {
            Program.current_path = Cmd.CurrentDirectory;
            if (Program.current_dir != null)
            {
                Program.current_dir = Program.current_dir.Parent;
                string str = new string(Program.current_dir.name).TrimEnd(new char[] { '\0' });
                Program.current_dir.Read_Directory();
                Console.WriteLine();
                var indexe= Program.current_path.Split('\\');
                for (int i = 0; i < indexe.Length - 1; i++)
                {
                    Cmd.CurrentDirectory = indexe[i];
                    if (i != 0)
                        Cmd.CurrentDirectory += '\\';
                }
                
            }

            else
                Console.WriteLine("You reached the root directory");

        }



        public static void dir()
        {
            Console.WriteLine($"\nDirectory of : {Cmd.CurrentDirectory}\n");
            int file_counter = 0;
            int dir_counter = 0;
            int files_size = 0;

            for (int i = 0; i < Program.current_dir.Directory_table.Count; i++)
            {
                if (Program.current_dir.Directory_table[i].attribute == 0x0)
                {
                    File_Entry f = new File_Entry(Program.current_dir.Directory_table[i].name, 0x0, Program.current_dir.Directory_table[i].first_cluster, Program.current_dir.Directory_table[i].size);
                    f.read_file();
                    string content = f.content.TrimEnd(new char[] { '*','0' });
                    string str = new string(Program.current_dir.Directory_table[i].name).TrimEnd(new char[] { '\0' });
                    Console.Write("{0,14:N0}", content.Length);
                    Console.WriteLine("\t" + str);
                    file_counter++;
                    files_size +=content.Length;
                }

                else if (Program.current_dir.Directory_table[i].attribute == 0x10)
                {
                    string str = new string(Program.current_dir.Directory_table[i].name).TrimEnd(new char[] { '\0' });
                    Console.WriteLine($"<Dir>\t\t{str}");
                    dir_counter++;
                }
            }
            Console.WriteLine($"\n  \t\t{file_counter} File(s)\t{files_size} bytes");
            Console.WriteLine($"    \t\t{dir_counter} Dir(s)\t{Fat_Table.get_free_space()} bytes free");

        }

        public static void import(string path)
        {
            if (File.Exists(path))
            {

                var indexe = path.Split('\\');

                string name = indexe[indexe.Length - 1];
                string content = File.ReadAllText(path);
                int size = content.Length;
                int index = Program.current_dir.Search(name);
                int fc = 0;

                if (index == -1)
                {
                 

                    File_Entry f = new File_Entry(name.ToCharArray(), 0x0, fc, size, content, Program.current_dir);
                    f.write_file();
                 

                    Directory_Entry d = new Directory_Entry(name.ToCharArray(), 0x0, f.first_cluster, size);
                    Program.current_dir.Directory_table.Add(d);
                    Program.current_dir.write_Directory();

                    if (Program.current_dir.Parent != null)
                        Program.current_dir.Parent.Update_Content(Program.current_dir.get_Directory_Entry());

                  
                    Console.WriteLine();
                }


                else
                    Console.WriteLine($"\nFile {name} already exists.\n");

            }
            else
                Console.WriteLine("\nThe system cannot find the path specified.\n");

        }

        public static void type(string name)
        {
            int index = Program.current_dir.Search(name);
         //   Console.WriteLine(name);
        //    Console.WriteLine(index);

            if (index != -1 && Program.current_dir.Directory_table[index].attribute == 0x0)
            {
             
                int fc = Program.current_dir.Directory_table[index].first_cluster;
                int size = Program.current_dir.Directory_table[index].size;
                string content="";
                File_Entry f = new File_Entry(name.ToCharArray(), 0x0, fc, size, content, Program.current_dir);
                f.read_file();

                content = f.content.TrimEnd(new char[] { '0','*' });
                Console.WriteLine("\n"+content+ "\n");
            }

           else
            Console.WriteLine("\nThe system cannot find the file specified.\n");
        }


        public static void export(string source,string destination)
        {
            int index = Program.current_dir.Search(source);

            if (index != -1 && Program.current_dir.Directory_table[index].attribute == 0x0)
            {
                if (System.IO.Directory.Exists(destination))
                {
                    int fc = Program.current_dir.Directory_table[index].first_cluster;
                    int size = Program.current_dir.Directory_table[index].size;
                    string content = "";
                    File_Entry f = new File_Entry(source.ToCharArray(), 0x0, fc, size, content,Program.current_dir);
                    f.read_file();
                    content = f.content.TrimEnd(new char[] { '*' });
                    StreamWriter sw = new StreamWriter(destination + "\\" + source);
                    sw.Write(content);
                    sw.Flush();
                    sw.Close();
                    Console.WriteLine();
                }

                else
                    Console.WriteLine("\nThe system cannot find the path specified at your computer.\n");
            }

            else
             Console.WriteLine("\nThe system cannot find the file specified.\n");

        }

        public static void rename(string nameO, string nameN)
        {

            int index = Program.current_dir.Search(nameO);
            int index2 = Program.current_dir.Search(nameN);

            if (index != -1)
            {
                if (index2 == -1)
                {
                    Directory_Entry d = Program.current_dir.Directory_table[index];
                    d.name = nameN.ToCharArray();
                    Program.current_dir.Directory_table.RemoveAt(index);
                    Program.current_dir.Directory_table.Insert(index, d);
                    Program.current_dir.write_Directory();

                    Console.WriteLine();

                }

                else
                    Console.WriteLine("\nA duplicate file name exists, or the file cannot be found.\n");

            }

            else
                Console.WriteLine("\nThe system cannot find the file specified.\n");
        }

        public static void del(string name)
        {
            int index = Program.current_dir.Search(name);
            if (index != -1 && Program.current_dir.Directory_table[index].attribute == 0x0)
            {
                int fc = Program.current_dir.Directory_table[index].first_cluster;
                int size = Program.current_dir.Directory_table[index].size;
                File_Entry f = new File_Entry(name.ToCharArray(), 0x0, fc, size,"", Program.current_dir);
                f.delete_file();
                Console.WriteLine();
                
            }
            else
                Console.WriteLine("\nThe system cannot find the file specified.\n");
        }

        public static void copy(string source, string dest)
        {
            var path = dest.Split('\\');
          //  Console.WriteLine(path[0]);
            int index = Program.current_dir.Search(source);
            int index2 = Program.current_dir.Search(path[0]);

          
            string fname = "";
            if (path.Length>0)
             fname = path[path.Length - 1];
           // Console.WriteLine(fname);
            if (index != -1 && index2 != -1 && Program.current_dir.Directory_table[index2].attribute == 0x10 && path.Length == 1)
            {
                Directory d = new Directory(dest.ToCharArray(), 0x10,
                    Program.current_dir.Directory_table[index2].first_cluster, Program.current_dir);
                d.Read_Directory();

                d.Directory_table.Add(Program.current_dir.Directory_table[index].get_Directory_Entry());
                d.write_Directory();

                if (d.Parent != null)
                {
                    d.Parent.Update_Content(d.get_Directory_Entry());
                    d.Parent.write_Directory();
                }

                Console.WriteLine("\n\t1 file<s> copied.\t\n");
            }

            else if(index != -1 && index2 != -1 && Program.current_dir.Directory_table[index2].attribute == 0x10 && fname.Length > 1)
            {
                int fc = Program.current_dir.Directory_table[index].first_cluster;
                int size = Program.current_dir.Directory_table[index].size;              
                File_Entry f = new File_Entry(Program.current_dir.Directory_table[index].name, 0x0, fc, size, "", Program.current_dir);
                f.read_file();
                string f_content = f.content;
                int f_sz = f.size;

                Directory d = new Directory(dest.ToCharArray(), 0x10,
                Program.current_dir.Directory_table[index2].first_cluster, Program.current_dir);
                d.Read_Directory();
                int index3 = d.Search(fname);
                if(index3 != -1)
                {
                    Console.Write($"\nOverwrite {fname} ? (Yes / No) :");
                    string x= Console.ReadLine().ToLower();
                    if (x == "y")
                    {
                        int fc2 =d.Directory_table[index3].first_cluster;
                     //   int size2 = d.Directory_table[index3].size;
                        File_Entry f2 = new File_Entry(fname.ToCharArray(), 0x0, fc, f_sz, "", d);
                        f2.content = f_content;
                    //    f2.size = f2.content.Length;                    

                        f2.write_file();

                        Directory_Entry df = new Directory_Entry(fname.ToCharArray(), 0x0, fc, f_sz);


                            d.Update_Content(df);
                            d.write_Directory();
                        

                        Console.WriteLine("\n\t1 file<s> copied.\t\n");
                    }

                    else if(x=="n")
                        Console.WriteLine("\n\t0 file<s> copied.\t\n");
                    else
                        Console.WriteLine("\nEntert y or n\n");
                }

                else
                    Console.WriteLine("\nThe system cannot find path specified.\n");
            }

            else
                Console.WriteLine("\nThe system cannot find path specified.\n");
        }

    }
    class Program
    {
            public static Directory current_dir;
            public static string current_path;

            static void Main(string[] args)
            {
                Virtual_Disk.Initialize();
                Cmd.Initialize(); 

            }

    }
 
}
using System.Threading;
using System.Threading.Tasks;

namespace ArtWiz.Utils
{
    public class TaskPool
    {
        private Task? newTask;
        private SemaphoreSlim executingSemaphoreSlim;
        public TaskPool(int cores)
        {
            executingSemaphoreSlim = new SemaphoreSlim(cores, cores);
        }

        /// <summary>
        /// Thêm task mới, task mới được thêm sẽ ghi đè lên task đang trong hàng đợi hiện tại.
        /// Khi task trước hoàn thành task mới sẽ được thưc hiện.
        /// 
        /// Giả sử hệ thống đang chạy task A, và có task B đang đợi A hoàn thành
        /// Task mới C được thêm vào, thì task B sẽ bị loại khỏi hàng đợi
        /// Khi task A hoàn thành sẽ đến lượt task C.
        /// </summary>
        /// <param name="task">Task cần thực hiện</param>
        public async void AddTaskToSinglePool(Task task)
        {
            newTask = task;
            await executingSemaphoreSlim.WaitAsync();
            if (task != newTask)
            {
                executingSemaphoreSlim.Release();
                return;
            }
            var currentTask = newTask;
            currentTask.Start();
            await currentTask;
            executingSemaphoreSlim.Release();
        }
    }
}

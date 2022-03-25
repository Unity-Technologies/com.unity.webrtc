#include <IUnityGraphicsVulkan.h>

namespace unity
{
namespace webrtc
{
    class UnityGraphicsVulkan
    {
    public:
        virtual void InterceptInitialization(UnityVulkanInitCallback func, void* userdata) = 0;

        static std::unique_ptr<UnityGraphicsVulkan> Get(IUnityInterfaces* unityInterfaces)
        {
            IUnityGraphicsVulkanV2* vulkanV2 = unityInterfaces->Get<IUnityGraphicsVulkanV2>();
            if (vulkanV2)
                return std::make_unique<UnityGraphicsVulkanImpl<IUnityGraphicsVulkanV2>>(vulkanV2);
            IUnityGraphicsVulkan* vulkan = unityInterfaces->Get<IUnityGraphicsVulkan>();
            if (vulkan)
                return std::make_unique<UnityGraphicsVulkanImpl<IUnityGraphicsVulkan>>(vulkan);
            return nullptr;
        }
    };


    template<class T>
    class UnityGraphicsVulkanImpl : public UnityGraphicsVulkan
    {
    public:
        UnityGraphicsVulkanImpl(T* vulkanInterface)
            : vulkanInterface_(vulkanInterface)
        {
        }
        ~UnityGraphicsVulkan() = default;

        void InterceptInitialization(UnityVulkanInitCallback func, void* userdata) override
        {
            vulkan.InterceptInitialization(func, nullptr);
        }

    private:
        T* vulkanInterface_;
    };
}
}

#pragma once
#include "WebRTCPlugin.h"

namespace unity
{
namespace webrtc
{

    class MediaStreamObserver : public webrtc::MediaStreamObserver, public sigslot::has_slots<>
    {
    public:
        explicit MediaStreamObserver(webrtc::MediaStreamInterface* stream);
        void RegisterOnAddTrack(DelegateMediaStreamOnAddTrack callback);
        void RegisterOnRemoveTrack(DelegateMediaStreamOnRemoveTrack callback);
        void RemoveCachedTrack(const char* trackId);
    private:
        void OnVideoTrackAdded(webrtc::VideoTrackInterface* track, webrtc::MediaStreamInterface* stream);
        void OnAudioTrackAdded(webrtc::AudioTrackInterface* track, webrtc::MediaStreamInterface* stream);
        void OnVideoTrackRemoved(webrtc::VideoTrackInterface* track, webrtc::MediaStreamInterface* stream);
        void OnAudioTrackRemoved(webrtc::AudioTrackInterface* track, webrtc::MediaStreamInterface* stream);

        std::list<DelegateMediaStreamOnAddTrack> m_listOnAddTrack;
        std::list<DelegateMediaStreamOnRemoveTrack> m_listOnRemoveTrack;

        std::vector<rtc::scoped_refptr<webrtc::AudioTrackInterface>> m_cachedAudioTrackList;
        std::vector<rtc::scoped_refptr<webrtc::VideoTrackInterface>> m_cachedVideoTrackList;
    };

} // end namespace webrtc
} // end namespace unity
